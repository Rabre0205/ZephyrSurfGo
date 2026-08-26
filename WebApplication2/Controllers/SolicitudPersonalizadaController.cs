using ClassLibrary.Servicios;
using ClassLibrary.Solicitudes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

public class SolicitudPersonalizadaController : Controller
{
    private readonly ISolicitudPersonalizadaServicio _servicio;
    public SolicitudPersonalizadaController(ISolicitudPersonalizadaServicio servicio) => _servicio = servicio;

    [Authorize(Roles = "Cliente")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear([FromForm] SolicitudPersonalizada solicitud)
    {
        var resultado = _servicio.Crear(ObtenerUsuarioId(), solicitud);
        return Json(new { creado = resultado.Exito, mensaje = resultado.Exito
            ? "Solicitud enviada al shaper para su revisión."
            : resultado.Error, solicitudId = resultado.Id });
    }

    [Authorize(Roles = "Cliente")]
    public IActionResult MisSolicitudes() => View(_servicio.ObtenerPorCliente(ObtenerUsuarioId()));

    [Authorize(Roles = "Cliente")]
    public IActionResult DetalleCliente(int id)
    {
        var solicitud = _servicio.ObtenerDetalleParaCliente(id, ObtenerUsuarioId());
        return solicitud == null ? NotFound() : View("Detalle", solicitud);
    }

    [Authorize(Roles = "Shaper")]
    public IActionResult Solicitudes() => View(_servicio.ObtenerPorShaper(ObtenerUsuarioId()));

    [Authorize(Roles = "Shaper")]
    public IActionResult DetalleShaper(int id)
    {
        var solicitud = _servicio.ObtenerDetalleParaShaper(id, ObtenerUsuarioId());
        return solicitud == null ? NotFound() : View("Detalle", solicitud);
    }

    [Authorize(Roles = "Shaper")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id, byte estado)
    {
        bool actualizado = _servicio.CambiarEstado(id, ObtenerUsuarioId(), estado);
        TempData[actualizado ? "Mensaje" : "Error"] = actualizado
            ? (estado == 1 ? "Solicitud marcada como revisada." : estado == 2
                ? "Solicitud marcada como no disponible." : "Estado actualizado.")
            : "No se pudo actualizar la solicitud.";
        return RedirectToAction(nameof(DetalleShaper), new { id });
    }

    private int ObtenerUsuarioId()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
            throw new InvalidOperationException("No se pudo identificar al usuario.");
        return id;
    }
}
