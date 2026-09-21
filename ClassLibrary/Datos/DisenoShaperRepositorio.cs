using ClassLibrary.Disenos;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface IDisenoShaperRepositorio
{
    List<DisenoShaper> ObtenerPorShaper(int shaperId, bool soloActivos = false);
    DisenoShaper? ObtenerPorId(int id);
    int Insertar(DisenoShaper diseno);
    bool Actualizar(DisenoShaper diseno);
    bool CambiarEstado(int id, int shaperId, bool activo);
}

public class DisenoShaperRepositorio : IDisenoShaperRepositorio
{
    public List<DisenoShaper> ObtenerPorShaper(int shaperId, bool soloActivos = false)
    {
        const string sql = @"SELECT * FROM DisenosShaper
            WHERE ShaperId=@ShaperId AND (@SoloActivos=0 OR Activo=1)
            ORDER BY Activo DESC, Nombre;";
        var lista = new List<DisenoShaper>();
        using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
        comando.Parameters.Add("@SoloActivos", SqlDbType.Bit).Value = soloActivos;
        conexion.Open(); using var lector = comando.ExecuteReader();
        while (lector.Read()) lista.Add(Mapear(lector));
        return lista;
    }

    public DisenoShaper? ObtenerPorId(int id)
    {
        using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand("SELECT * FROM DisenosShaper WHERE Id=@Id", conexion);
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        conexion.Open(); using var lector = comando.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    public int Insertar(DisenoShaper d)
    {
        const string sql = @"INSERT INTO DisenosShaper
            (ShaperId,Nombre,Descripcion,ImagenUrl,ZonaAplicacion,PermiteColoresPersonalizados,ColorPrimario,ColorSecundario,Recargo,Activo)
            OUTPUT INSERTED.Id VALUES
            (@ShaperId,@Nombre,@Descripcion,@ImagenUrl,@Zona,@PermiteColores,@ColorPrimario,@ColorSecundario,@Recargo,1);";
        using var conexion = Conexion.ObtenerConexion(); using var comando = new SqlCommand(sql, conexion);
        Parametros(comando,d); conexion.Open(); return Convert.ToInt32(comando.ExecuteScalar());
    }

    public bool Actualizar(DisenoShaper d)
    {
        const string sql = @"UPDATE DisenosShaper SET Nombre=@Nombre,Descripcion=@Descripcion,
            ImagenUrl=@ImagenUrl,ZonaAplicacion=@Zona,PermiteColoresPersonalizados=@PermiteColores,
            ColorPrimario=@ColorPrimario,ColorSecundario=@ColorSecundario,
            Recargo=@Recargo,FechaActualizacion=SYSUTCDATETIME()
            WHERE Id=@Id AND ShaperId=@ShaperId;";
        using var conexion = Conexion.ObtenerConexion(); using var comando = new SqlCommand(sql, conexion);
        Parametros(comando,d); comando.Parameters.Add("@Id",SqlDbType.Int).Value=d.Id;
        conexion.Open(); return comando.ExecuteNonQuery()==1;
    }

    public bool CambiarEstado(int id,int shaperId,bool activo)
    {
        const string sql="UPDATE DisenosShaper SET Activo=@Activo,FechaActualizacion=SYSUTCDATETIME() WHERE Id=@Id AND ShaperId=@ShaperId";
        using var conexion=Conexion.ObtenerConexion();using var comando=new SqlCommand(sql,conexion);
        comando.Parameters.Add("@Activo",SqlDbType.Bit).Value=activo;comando.Parameters.Add("@Id",SqlDbType.Int).Value=id;comando.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId;
        conexion.Open();return comando.ExecuteNonQuery()==1;
    }

    private static void Parametros(SqlCommand c,DisenoShaper d)
    {
        c.Parameters.Add("@ShaperId",SqlDbType.Int).Value=d.ShaperId;c.Parameters.Add("@Nombre",SqlDbType.NVarChar,120).Value=d.Nombre;
        c.Parameters.Add("@Descripcion",SqlDbType.NVarChar,600).Value=d.Descripcion;c.Parameters.Add("@ImagenUrl",SqlDbType.NVarChar,500).Value=(object?)d.ImagenUrl??DBNull.Value;
        c.Parameters.Add("@Zona",SqlDbType.NVarChar,20).Value=d.ZonaAplicacion;c.Parameters.Add("@PermiteColores",SqlDbType.Bit).Value=d.PermiteColoresPersonalizados;
        c.Parameters.Add("@ColorPrimario",SqlDbType.NVarChar,7).Value=d.ColorPrimario;c.Parameters.Add("@ColorSecundario",SqlDbType.NVarChar,7).Value=d.ColorSecundario;
        var recargo=c.Parameters.Add("@Recargo",SqlDbType.Decimal);recargo.Precision=10;recargo.Scale=2;recargo.Value=d.Recargo;
    }
    private static DisenoShaper Mapear(SqlDataReader r)=>new(){Id=Convert.ToInt32(r["Id"]),ShaperId=Convert.ToInt32(r["ShaperId"]),Nombre=Convert.ToString(r["Nombre"])??"",Descripcion=Convert.ToString(r["Descripcion"])??"",ImagenUrl=r["ImagenUrl"]==DBNull.Value?null:Convert.ToString(r["ImagenUrl"]),ZonaAplicacion=Convert.ToString(r["ZonaAplicacion"])??"Ambos",PermiteColoresPersonalizados=Convert.ToBoolean(r["PermiteColoresPersonalizados"]),ColorPrimario=Convert.ToString(r["ColorPrimario"])??"#ffffff",ColorSecundario=Convert.ToString(r["ColorSecundario"])??"#111111",Recargo=Convert.ToDecimal(r["Recargo"]),Activo=Convert.ToBoolean(r["Activo"]),FechaCreacion=Convert.ToDateTime(r["FechaCreacion"])};
}
