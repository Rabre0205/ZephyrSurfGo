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
    (bool Exito, string Error) DefinirPrecio(int id, int shaperId, decimal precio);
    (bool Exito, string Error) ResponderCotizacion(int id, int clienteId, bool aceptar);
}

public class SolicitudPersonalizadaServicio : ISolicitudPersonalizadaServicio
{
    private readonly ISolicitudPersonalizadaRepositorio _repositorio;
    private readonly IProductoRepositorio _productos;
    private readonly IUsuarioRepositorio _usuarios;

    public SolicitudPersonalizadaServicio(
        ISolicitudPersonalizadaRepositorio repositorio,
        IProductoRepositorio productos,
        IUsuarioRepositorio usuarios)
    {
        _repositorio = repositorio;
        _productos = productos;
        _usuarios = usuarios;
    }

    public (bool Exito, string Error, int Id) Crear(
        int clienteId, SolicitudPersonalizada solicitud)
    {
        Producto? producto = solicitud.ProductoBaseId.HasValue
            ? _productos.ObtenerPorId(solicitud.ProductoBaseId.Value)
            : null;

        if (solicitud.ProductoBaseId.HasValue && producto is not Tabla)
            return (false, "El modelo base seleccionado no es válido.", 0);

        if (producto is Tabla tabla)
        {
            solicitud.ShaperId = tabla.ShaperId;
            solicitud.Modelo = tabla.Titulo;
            if (solicitud.PrecioEstimado < (decimal)tabla.Precio ||
                solicitud.PrecioEstimado > (decimal)tabla.Precio + 5000m)
                return (false, "El precio estimado de la personalización no es válido.", 0);
        }
        else
        {
            if (_usuarios.ObtenerPorId(solicitud.ShaperId) is not ClassLibrary.Persona.Shaper shaper || !shaper.Activo)
                return (false, "El shaper seleccionado no está disponible.", 0);
            if (solicitud.PrecioEstimado < 0 || solicitud.PrecioEstimado > 10000m)
                return (false, "El precio estimado de la personalización no es válido.", 0);
            if (string.IsNullOrWhiteSpace(solicitud.Modelo))
                solicitud.Modelo = "Tabla personalizada";
        }

        if (string.IsNullOrWhiteSpace(solicitud.Largo) ||
            string.IsNullOrWhiteSpace(solicitud.Volumen))
            return (false, "Completá las medidas principales de la tabla.", 0);

        solicitud.AccesoriosJson ??= "[]";
        if (solicitud.AccesoriosJson.Length > 10000 || !EsJsonValido(solicitud.AccesoriosJson))
            return (false, "La selección de accesorios no es válida.", 0);

        solicitud.ClienteId = clienteId;
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

    public (bool Exito, string Error) DefinirPrecio(int id, int shaperId, decimal precio)
    {
        if (precio < 1m || precio > 100000m)
            return (false, "Ingresá un precio válido entre USD 1 y USD 100.000.");

        var pedido = ObtenerDetalleParaShaper(id, shaperId);
        if (pedido == null)
            return (false, "No se encontró el pedido personalizado.");
        if (pedido.Estado == 2)
            return (false, "No se puede cotizar un pedido marcado como no disponible.");

        return _repositorio.DefinirPrecio(id, shaperId, decimal.Round(precio, 2))
            ? (true, string.Empty)
            : (false, "No se pudo guardar el precio.");
    }

    public (bool Exito, string Error) ResponderCotizacion(
        int id, int clienteId, bool aceptar)
    {
        var pedido = ObtenerDetalleParaCliente(id, clienteId);
        if (pedido == null)
            return (false, "No se encontró el pedido personalizado.");
        if (pedido.Estado != 1 || pedido.PrecioEstimado <= 0)
            return (false, "Esta cotización ya no está disponible para responder.");

        return _repositorio.ResponderCotizacion(id, clienteId, aceptar)
            ? (true, string.Empty)
            : (false, "No se pudo registrar tu respuesta. Actualizá la página e intentá nuevamente.");
    }

    private static bool EsJsonValido(string texto)
    {
        try { JsonDocument.Parse(texto); return true; }
        catch (JsonException) { return false; }
    }

    private static void LimitarTextos(SolicitudPersonalizada s)
    {
        s.Modelo = Limitar(s.Modelo, 150);
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
