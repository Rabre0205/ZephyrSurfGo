using ClassLibrary.Enums;
using ClassLibrary.PuntosRetiro;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface IPuntoRetiroRepositorio
{
    List<PuntoRetiro> ObtenerPorShaper(int shaperId);
    List<PuntoRetiro> ObtenerActivos();
    PuntoRetiro? ObtenerPorId(int id);
    int Insertar(PuntoRetiro punto);
    bool Actualizar(PuntoRetiro punto);
    bool CambiarEstado(int id, int shaperId, bool activo);
}

public class PuntoRetiroRepositorio : IPuntoRetiroRepositorio
{
    public List<PuntoRetiro> ObtenerPorShaper(int shaperId) => ObtenerLista("p.ShaperId = @ShaperId", shaperId);
    public List<PuntoRetiro> ObtenerActivos() => ObtenerLista("p.Activo = 1 AND u.Activo = 1", null);

    public PuntoRetiro? ObtenerPorId(int id)
    {
        using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand(Seleccion + " WHERE p.Id = @Id;", conexion);
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        conexion.Open();
        using var lector = comando.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    public int Insertar(PuntoRetiro p)
    {
        const string sql = @"INSERT INTO PuntosRetiro
            (ShaperId, Nombre, Direccion, Ciudad, Horario, Indicaciones, Latitud, Longitud, Activo)
            OUTPUT INSERTED.Id VALUES
            (@ShaperId, @Nombre, @Direccion, @Ciudad, @Horario, @Indicaciones, @Latitud, @Longitud, 1);";
        using var conexion = Conexion.ObtenerConexion(); using var comando = new SqlCommand(sql, conexion);
        Parametros(comando, p); conexion.Open(); return Convert.ToInt32(comando.ExecuteScalar());
    }

    public bool Actualizar(PuntoRetiro p)
    {
        const string sql = @"UPDATE PuntosRetiro SET Nombre=@Nombre, Direccion=@Direccion,
            Ciudad=@Ciudad, Horario=@Horario, Indicaciones=@Indicaciones,
            Latitud=@Latitud, Longitud=@Longitud, FechaActualizacion=SYSUTCDATETIME()
            WHERE Id=@Id AND ShaperId=@ShaperId;";
        using var conexion = Conexion.ObtenerConexion(); using var comando = new SqlCommand(sql, conexion);
        Parametros(comando, p); comando.Parameters.Add("@Id", SqlDbType.Int).Value = p.Id;
        conexion.Open(); return comando.ExecuteNonQuery() == 1;
    }

    public bool CambiarEstado(int id, int shaperId, bool activo)
    {
        const string sql = "UPDATE PuntosRetiro SET Activo=@Activo, FechaActualizacion=SYSUTCDATETIME() WHERE Id=@Id AND ShaperId=@ShaperId;";
        using var conexion = Conexion.ObtenerConexion(); using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
        conexion.Open(); return comando.ExecuteNonQuery() == 1;
    }

    private static List<PuntoRetiro> ObtenerLista(string filtro, int? shaperId)
    {
        var lista = new List<PuntoRetiro>(); using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand(Seleccion + $" WHERE {filtro} ORDER BY p.Nombre;", conexion);
        if (shaperId.HasValue) comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId.Value;
        conexion.Open(); using var lector = comando.ExecuteReader(); while (lector.Read()) lista.Add(Mapear(lector)); return lista;
    }

    private const string Seleccion = @"SELECT p.*, u.Nombre ShaperNombre, u.PaisId, u.NombreDeNegosio, u.LogoUrl
        FROM PuntosRetiro p INNER JOIN Usuarios u ON u.Id=p.ShaperId";
    private static void Parametros(SqlCommand c, PuntoRetiro p)
    {
        c.Parameters.Add("@ShaperId", SqlDbType.Int).Value=p.ShaperId;
        c.Parameters.Add("@Nombre", SqlDbType.NVarChar,150).Value=p.Nombre;
        c.Parameters.Add("@Direccion", SqlDbType.NVarChar,250).Value=p.Direccion;
        c.Parameters.Add("@Ciudad", SqlDbType.NVarChar,120).Value=p.Ciudad;
        c.Parameters.Add("@Horario", SqlDbType.NVarChar,250).Value=p.Horario;
        c.Parameters.Add("@Indicaciones", SqlDbType.NVarChar,500).Value=p.Indicaciones;
        c.Parameters.Add("@Latitud", SqlDbType.Decimal).Value=p.Latitud; c.Parameters["@Latitud"].Precision=9; c.Parameters["@Latitud"].Scale=6;
        c.Parameters.Add("@Longitud", SqlDbType.Decimal).Value=p.Longitud; c.Parameters["@Longitud"].Precision=9; c.Parameters["@Longitud"].Scale=6;
    }
    private static PuntoRetiro Mapear(SqlDataReader r)
    {
        string negocio = Convert.ToString(r["NombreDeNegosio"]) ?? string.Empty;
        return new()
    {
        Id=Convert.ToInt32(r["Id"]), ShaperId=Convert.ToInt32(r["ShaperId"]),
        ShaperNombre=string.IsNullOrWhiteSpace(negocio) ? Convert.ToString(r["ShaperNombre"]) ?? "" : negocio,
        LogoUrl=r["LogoUrl"]==DBNull.Value ? null : Convert.ToString(r["LogoUrl"]), Nombre=Convert.ToString(r["Nombre"]) ?? "",
        Direccion=Convert.ToString(r["Direccion"]) ?? "", Ciudad=Convert.ToString(r["Ciudad"]) ?? "",
        Horario=Convert.ToString(r["Horario"]) ?? "", Indicaciones=Convert.ToString(r["Indicaciones"]) ?? "",
        Latitud=Convert.ToDecimal(r["Latitud"]), Longitud=Convert.ToDecimal(r["Longitud"]),
        Activo=Convert.ToBoolean(r["Activo"]), Pais=(Pais)Convert.ToInt32(r["PaisId"]), FechaCreacion=Convert.ToDateTime(r["FechaCreacion"])
    };
    }
}
