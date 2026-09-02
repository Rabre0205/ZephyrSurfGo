using ClassLibrary.Servicios;
using ClassLibrary.Solicitudes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Controllers;

namespace Pruebas;

public class SolicitudPersonalizadaControllerTests
{
    [Fact]
    public void CrearUsaElClienteAutenticado()
    {
        var servicio = new ServicioFalso { ResultadoCrear = (true, "", 17) };
        var controller = CrearController(servicio, 42, "Cliente");

        var resultado = Assert.IsType<JsonResult>(controller.Crear(new SolicitudPersonalizada { ClienteId = 999 }));

        Assert.Equal(42, servicio.UltimoUsuarioId);
        Assert.Equal(17, Leer<int>(resultado.Value!, "solicitudId"));
    }

    [Fact]
    public void DetalleShaperConsultaConElShaperAutenticado()
    {
        var servicio = new ServicioFalso { Detalle = new SolicitudPersonalizada { Id = 8 } };
        var controller = CrearController(servicio, 73, "Shaper");

        var resultado = Assert.IsType<ViewResult>(controller.DetalleShaper(8));

        Assert.Equal("Detalle", resultado.ViewName);
        Assert.Equal(73, servicio.UltimoUsuarioId);
    }

    [Fact]
    public void CambiarEstadoNoAceptaUnShaperEnviadoPorFormulario()
    {
        var servicio = new ServicioFalso { ResultadoCambio = true };
        var controller = CrearController(servicio, 55, "Shaper");

        controller.CambiarEstado(4, 1);

        Assert.Equal(55, servicio.UltimoUsuarioId);
        Assert.Equal((byte)1, servicio.UltimoEstado);
    }

    private static SolicitudPersonalizadaController CrearController(ServicioFalso servicio, int usuarioId, string rol)
    {
        var controller = new SolicitudPersonalizadaController(servicio);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()), new Claim(ClaimTypes.Role, rol)
        ], "Prueba")) } };
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            controller.HttpContext, new TempDataProviderFalso());
        return controller;
    }

    private static T Leer<T>(object value, string nombre) =>
        (T)value.GetType().GetProperty(nombre)!.GetValue(value)!;

    private sealed class TempDataProviderFalso : Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class ServicioFalso : ISolicitudPersonalizadaServicio
    {
        public int UltimoUsuarioId { get; private set; }
        public byte UltimoEstado { get; private set; }
        public (bool Exito, string Error, int Id) ResultadoCrear { get; set; }
        public bool ResultadoCambio { get; set; }
        public SolicitudPersonalizada? Detalle { get; set; }
        public (bool Exito, string Error, int Id) Crear(int clienteId, SolicitudPersonalizada solicitud) { UltimoUsuarioId = clienteId; return ResultadoCrear; }
        public List<SolicitudPersonalizada> ObtenerPorShaper(int shaperId) { UltimoUsuarioId = shaperId; return []; }
        public List<SolicitudPersonalizada> ObtenerPorCliente(int clienteId) { UltimoUsuarioId = clienteId; return []; }
        public SolicitudPersonalizada? ObtenerDetalleParaShaper(int id, int shaperId) { UltimoUsuarioId = shaperId; return Detalle; }
        public SolicitudPersonalizada? ObtenerDetalleParaCliente(int id, int clienteId) { UltimoUsuarioId = clienteId; return Detalle; }
        public bool CambiarEstado(int id, int shaperId, byte estado) { UltimoUsuarioId = shaperId; UltimoEstado = estado; return ResultadoCambio; }
        public (bool Exito, string Error) DefinirPrecio(int id, int shaperId, decimal precio) { UltimoUsuarioId = shaperId; return (true, string.Empty); }
        public (bool Exito, string Error) ResponderCotizacion(int id, int clienteId, bool aceptar) { UltimoUsuarioId = clienteId; return (true, string.Empty); }
    }
}
