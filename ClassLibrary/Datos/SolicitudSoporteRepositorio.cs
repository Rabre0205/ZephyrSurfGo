using ClassLibrary.Soporte;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface ISolicitudSoporteRepositorio
{
    int Insertar(SolicitudSoporte solicitud);
    List<SolicitudSoporte> ObtenerPorShaper(int shaperId);
    List<SolicitudSoporte> ObtenerTodas();
    SolicitudSoporte? ObtenerPorId(int id);
    bool Responder(int id, string respuesta, byte estado);
}

public class SolicitudSoporteRepositorio : ISolicitudSoporteRepositorio
{
    public int Insertar(SolicitudSoporte s)
    {
        const string sql = @"INSERT INTO SolicitudesSoporte (ShaperId, Asunto, Mensaje)
            OUTPUT INSERTED.Id VALUES (@ShaperId,@Asunto,@Mensaje);";
        using var conexion=Conexion.ObtenerConexion(); using var comando=new SqlCommand(sql,conexion);
        comando.Parameters.Add("@ShaperId",SqlDbType.Int).Value=s.ShaperId;
        comando.Parameters.Add("@Asunto",SqlDbType.NVarChar,150).Value=s.Asunto;
        comando.Parameters.Add("@Mensaje",SqlDbType.NVarChar,2000).Value=s.Mensaje;
        conexion.Open(); return Convert.ToInt32(comando.ExecuteScalar());
    }
    public List<SolicitudSoporte> ObtenerPorShaper(int shaperId)=>Lista("s.ShaperId=@ShaperId",shaperId);
    public List<SolicitudSoporte> ObtenerTodas()=>Lista("1=1",null);
    public SolicitudSoporte? ObtenerPorId(int id)
    {
        using var conexion=Conexion.ObtenerConexion(); using var comando=new SqlCommand(Seleccion+" WHERE s.Id=@Id;",conexion);
        comando.Parameters.Add("@Id",SqlDbType.Int).Value=id; conexion.Open(); using var lector=comando.ExecuteReader();
        return lector.Read()?Mapear(lector):null;
    }
    public bool Responder(int id,string respuesta,byte estado)
    {
        const string sql=@"UPDATE SolicitudesSoporte SET Respuesta=@Respuesta, Estado=@Estado,
            FechaRespuesta=SYSUTCDATETIME() WHERE Id=@Id;";
        using var conexion=Conexion.ObtenerConexion(); using var comando=new SqlCommand(sql,conexion);
        comando.Parameters.Add("@Respuesta",SqlDbType.NVarChar,2000).Value=respuesta;
        comando.Parameters.Add("@Estado",SqlDbType.TinyInt).Value=estado;
        comando.Parameters.Add("@Id",SqlDbType.Int).Value=id; conexion.Open(); return comando.ExecuteNonQuery()==1;
    }
    private static List<SolicitudSoporte> Lista(string filtro,int? shaperId)
    {
        var lista=new List<SolicitudSoporte>(); using var conexion=Conexion.ObtenerConexion();
        using var comando=new SqlCommand(Seleccion+$" WHERE {filtro} ORDER BY s.FechaCreacion DESC;",conexion);
        if(shaperId.HasValue)comando.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId.Value;
        conexion.Open(); using var lector=comando.ExecuteReader(); while(lector.Read())lista.Add(Mapear(lector)); return lista;
    }
    private const string Seleccion=@"SELECT s.*,u.Nombre ShaperNombre,u.Email ShaperEmail FROM SolicitudesSoporte s INNER JOIN Usuarios u ON u.Id=s.ShaperId";
    private static SolicitudSoporte Mapear(SqlDataReader r)=>new(){Id=Convert.ToInt32(r["Id"]),ShaperId=Convert.ToInt32(r["ShaperId"]),
        ShaperNombre=Convert.ToString(r["ShaperNombre"])??"",ShaperEmail=Convert.ToString(r["ShaperEmail"])??"",
        Asunto=Convert.ToString(r["Asunto"])??"",Mensaje=Convert.ToString(r["Mensaje"])??"",
        Respuesta=r["Respuesta"]==DBNull.Value?null:Convert.ToString(r["Respuesta"]),Estado=Convert.ToByte(r["Estado"]),
        FechaCreacion=Convert.ToDateTime(r["FechaCreacion"]),FechaRespuesta=r["FechaRespuesta"]==DBNull.Value?null:Convert.ToDateTime(r["FechaRespuesta"])};
}
