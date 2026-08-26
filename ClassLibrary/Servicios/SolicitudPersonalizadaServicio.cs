using ClassLibrary.Datos;
using ClassLibrary.Productos;
using ClassLibrary.Solicitudes;
using System.Text.Json;

namespace ClassLibrary.Servicios;

public interface ISolicitudPersonalizadaServicio
{
    (bool Exito, string Error, int Id) Crear(int clienteId, SolicitudPersonalizada solicitud);
    List<SolicitudPersonalizada> ObtenerPorShaper(int shaperId);
    List<SolicitudPersonalizada> ObtenerPorCliente(int clienteId);
    SolicitudPersonalizada? ObtenerDetalleParaShaper(int id, int shaperId);
    SolicitudPersonalizada? ObtenerDetalleParaCliente(int id, int clienteId);
    bool CambiarEstado(int id, int shaperId, byte estado);
}

public class SolicitudPersonalizadaServicio : ISolicitudPersonalizadaServicio
{
    private readonly ISolicitudPersonalizadaRepositorio _repositorio;
    private readonly IProductoRepositorio _productos;

    public SolicitudPersonalizadaServicio(
        ISolicitudPersonalizadaRepositorio repositorio,
        IProductoRepositorio productos)
    {
        _repositorio = repositorio;
        _productos = productos;
    }

    public (bool Exito, string Error, int Id) Crear(
        int clienteId, SolicitudPersonalizada solicitud)
    {
        Producto? producto = _productos.ObtenerPorId(solicitud.ProductoBaseId);
        if (producto is not Tabla)
            return (false, "Seleccioná una tabla publicada por el shaper.", 0);

        if (solicitud.PrecioEstimado < (decimal)producto.Precio ||
            solicitud.PrecioEstimado > (decimal)producto.Precio + 5000m)
            return (false, "El precio estimado de la personalización no es válido.", 0);

        if (string.IsNullOrWhiteSpace(solicitud.Largo) ||
            string.IsNullOrWhiteSpace(solicitud.Volumen))
            return (false, "Completá las medidas principales de la tabla.", 0);

        solicitud.AccesoriosJson ??= "[]";
        if (solicitud.AccesoriosJson.Length > 10000 || !EsJsonValido(solicitud.AccesoriosJson))
            return (false, "La selección de accesorios no es válida.", 0);

        solicitud.ClienteId = clienteId;
        solicitud.ShaperId = producto.ShaperId;
        solicitud.Modelo = producto.Titulo;
        LimitarTextos(solicitud);
        int id = _repositorio.Insertar(solicitud);
        return id > 0
            ? (true, string.Empty, id)
            : (false, "No se pudo registrar la solicitud.", 0);
    }

    public List<SolicitudPersonalizada> ObtenerPorShaper(int shaperId) =>
        _repositorio.ObtenerPorShaper(shaperId);
    public List<SolicitudPersonalizada> ObtenerPorCliente(int clienteId) =>
        _repositorio.ObtenerPorCliente(clienteId);

    public SolicitudPersonalizada? ObtenerDetalleParaShaper(int id, int shaperId)
    {
        var solicitud = _repositorio.ObtenerDetalle(id);
        return solicitud?.ShaperId == shaperId ? solicitud : null;
    }

    public SolicitudPersonalizada? ObtenerDetalleParaCliente(int id, int clienteId)
    {
        var solicitud = _repositorio.ObtenerDetalle(id);
        return solicitud?.ClienteId == clienteId ? solicitud : null;
    }

    public bool CambiarEstado(int id, int shaperId, byte estado) =>
        estado <= 2 && _repositorio.CambiarEstado(id, shaperId, estado);

    private static bool EsJsonValido(string texto)
    {
        try { JsonDocument.Parse(texto); return true; }
        catch (JsonException) { return false; }
    }

    private static void LimitarTextos(SolicitudPersonalizada s)
    {
        s.Largo = Limitar(s.Largo, 30); s.Ancho = Limitar(s.Ancho, 30);
        s.Grosor = Limitar(s.Grosor, 30); s.Volumen = Limitar(s.Volumen, 30);
        s.Construccion = Limitar(s.Construccion, 100); s.Tail = Limitar(s.Tail, 80);
        s.SistemaQuillas = Limitar(s.SistemaQuillas, 80);
        s.ConfiguracionQuillas = Limitar(s.ConfiguracionQuillas, 100);
        s.Laminado = Limitar(s.Laminado, 100); s.ParcheCarbono = Limitar(s.ParcheCarbono, 100);
        s.Diseno = Limitar(s.Diseno, 100); s.ColorPrimario = Limitar(s.ColorPrimario, 30);
        s.ColorSecundario = Limitar(s.ColorSecundario, 30);
        s.DetallesAdicionales = Limitar(s.DetallesAdicionales, 500);
        s.Notas = Limitar(s.Notas, 1000);
    }

    private static string Limitar(string? texto, int maximo) =>
        string.IsNullOrWhiteSpace(texto) ? string.Empty : texto.Trim()[..Math.Min(texto.Trim().Length, maximo)];
}
