using ClassLibrary.Solicitudes;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface ISolicitudPersonalizadaRepositorio
{
    int Insertar(SolicitudPersonalizada solicitud);
    List<SolicitudPersonalizada> ObtenerPorShaper(int shaperId);
    List<SolicitudPersonalizada> ObtenerPorCliente(int clienteId);
    SolicitudPersonalizada? ObtenerDetalle(int id);
    bool CambiarEstado(int id, int shaperId, byte estado);
}

public class SolicitudPersonalizadaRepositorio : ISolicitudPersonalizadaRepositorio
{
    public int Insertar(SolicitudPersonalizada s)
    {
        const string sql = @"
            INSERT INTO SolicitudesPersonalizadas
            (ClienteId, ShaperId, ProductoBaseId, ModeloSnapshot, PrecioEstimado,
             Largo, Ancho, Grosor, Volumen, Construccion, Tail, SistemaQuillas,
             ConfiguracionQuillas, Laminado, ParcheCarbono, Diseno, ColorPrimario,
             ColorSecundario, DetallesAdicionales, AccesoriosJson, Notas)
            OUTPUT INSERTED.Id
            VALUES
            (@ClienteId, @ShaperId, @ProductoBaseId, @Modelo, @PrecioEstimado,
             @Largo, @Ancho, @Grosor, @Volumen, @Construccion, @Tail, @SistemaQuillas,
             @ConfiguracionQuillas, @Laminado, @ParcheCarbono, @Diseno, @ColorPrimario,
             @ColorSecundario, @DetallesAdicionales, @AccesoriosJson, @Notas);";

        using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand(sql, conexion);
        AgregarParametros(comando, s);
        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public List<SolicitudPersonalizada> ObtenerPorShaper(int shaperId) =>
        ObtenerLista("s.ShaperId = @UsuarioId", shaperId);

    public List<SolicitudPersonalizada> ObtenerPorCliente(int clienteId) =>
        ObtenerLista("s.ClienteId = @UsuarioId", clienteId);

    public SolicitudPersonalizada? ObtenerDetalle(int id)
    {
        string sql = SeleccionBase + " WHERE s.Id = @Id;";
        using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        conexion.Open();
        using var lector = comando.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    public bool CambiarEstado(int id, int shaperId, byte estado)
    {
        const string sql = @"
            UPDATE SolicitudesPersonalizadas
            SET Estado = @Estado, FechaActualizacion = SYSUTCDATETIME()
            WHERE Id = @Id AND ShaperId = @ShaperId;";
        using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@Estado", SqlDbType.TinyInt).Value = estado;
        comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
        conexion.Open();
        return comando.ExecuteNonQuery() == 1;
    }

    private List<SolicitudPersonalizada> ObtenerLista(string filtro, int usuarioId)
    {
        string sql = SeleccionBase + $" WHERE {filtro} ORDER BY s.FechaCreacion DESC;";
        var resultado = new List<SolicitudPersonalizada>();
        using var conexion = Conexion.ObtenerConexion();
        using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
        conexion.Open();
        using var lector = comando.ExecuteReader();
        while (lector.Read()) resultado.Add(Mapear(lector));
        return resultado;
    }

    private const string SeleccionBase = @"
        SELECT s.*, c.Nombre ClienteNombre, c.Email ClienteEmail,
               sh.Nombre ShaperNombre
        FROM SolicitudesPersonalizadas s
        INNER JOIN Usuarios c ON c.Id = s.ClienteId
        INNER JOIN Usuarios sh ON sh.Id = s.ShaperId";

    private static void AgregarParametros(SqlCommand c, SolicitudPersonalizada s)
    {
        c.Parameters.Add("@ClienteId", SqlDbType.Int).Value = s.ClienteId;
        c.Parameters.Add("@ShaperId", SqlDbType.Int).Value = s.ShaperId;
        c.Parameters.Add("@ProductoBaseId", SqlDbType.Int).Value = s.ProductoBaseId;
        c.Parameters.Add("@Modelo", SqlDbType.NVarChar, 150).Value = s.Modelo;
        c.Parameters.Add("@PrecioEstimado", SqlDbType.Decimal).Value = s.PrecioEstimado;
        foreach (var (nombre, valor, largo) in new[] {
            ("Largo",s.Largo,30),("Ancho",s.Ancho,30),("Grosor",s.Grosor,30),("Volumen",s.Volumen,30),
            ("Construccion",s.Construccion,100),("Tail",s.Tail,80),("SistemaQuillas",s.SistemaQuillas,80),
            ("ConfiguracionQuillas",s.ConfiguracionQuillas,100),("Laminado",s.Laminado,100),
            ("ParcheCarbono",s.ParcheCarbono,100),("Diseno",s.Diseno,100),
            ("ColorPrimario",s.ColorPrimario,30),("ColorSecundario",s.ColorSecundario,30),
            ("DetallesAdicionales",s.DetallesAdicionales,500),("Notas",s.Notas,1000) })
            c.Parameters.Add("@" + nombre, SqlDbType.NVarChar, largo).Value = valor ?? string.Empty;
        c.Parameters.Add("@AccesoriosJson", SqlDbType.NVarChar, -1).Value = s.AccesoriosJson ?? "[]";
    }

    private static SolicitudPersonalizada Mapear(SqlDataReader r) => new()
    {
        Id = Convert.ToInt32(r["Id"]), ClienteId = Convert.ToInt32(r["ClienteId"]),
        ShaperId = Convert.ToInt32(r["ShaperId"]), ProductoBaseId = Convert.ToInt32(r["ProductoBaseId"]),
        ClienteNombre = Convert.ToString(r["ClienteNombre"]) ?? "", ClienteEmail = Convert.ToString(r["ClienteEmail"]) ?? "",
        ShaperNombre = Convert.ToString(r["ShaperNombre"]) ?? "", Modelo = Convert.ToString(r["ModeloSnapshot"]) ?? "",
        PrecioEstimado = Convert.ToDecimal(r["PrecioEstimado"]), Largo = Convert.ToString(r["Largo"]) ?? "",
        Ancho = Convert.ToString(r["Ancho"]) ?? "", Grosor = Convert.ToString(r["Grosor"]) ?? "",
        Volumen = Convert.ToString(r["Volumen"]) ?? "", Construccion = Convert.ToString(r["Construccion"]) ?? "",
        Tail = Convert.ToString(r["Tail"]) ?? "", SistemaQuillas = Convert.ToString(r["SistemaQuillas"]) ?? "",
        ConfiguracionQuillas = Convert.ToString(r["ConfiguracionQuillas"]) ?? "", Laminado = Convert.ToString(r["Laminado"]) ?? "",
        ParcheCarbono = Convert.ToString(r["ParcheCarbono"]) ?? "", Diseno = Convert.ToString(r["Diseno"]) ?? "",
        ColorPrimario = Convert.ToString(r["ColorPrimario"]) ?? "", ColorSecundario = Convert.ToString(r["ColorSecundario"]) ?? "",
        DetallesAdicionales = Convert.ToString(r["DetallesAdicionales"]) ?? "", AccesoriosJson = Convert.ToString(r["AccesoriosJson"]) ?? "[]",
        Notas = Convert.ToString(r["Notas"]) ?? "", Estado = Convert.ToByte(r["Estado"]),
        FechaCreacion = Convert.ToDateTime(r["FechaCreacion"])
    };
}
