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
            ? "Pedido personalizado enviado al shaper para su revisión."
            : resultado.Error, solicitudId = resultado.Id });
    }

    [Authorize(Roles = "Cliente")]
    public IActionResult MisSolicitudes() => RedirectToAction("Index", "MisPedidos");

    [Authorize(Roles = "Cliente")]
    public IActionResult DetalleCliente(int id)
    {
        var solicitud = _servicio.ObtenerDetalleParaCliente(id, ObtenerUsuarioId());
        return solicitud == null ? NotFound() : View("Detalle", solicitud);
    }

    [Authorize(Roles = "Shaper")]
    public IActionResult Solicitudes() => RedirectToAction("Pedidos", "Dashboard");

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
            ? (estado == 1 ? "Pedido personalizado marcado como revisado." : estado == 2
                ? "Pedido personalizado marcado como no disponible." : "Estado actualizado.")
            : "No se pudo actualizar el pedido personalizado.";
        return RedirectToAction(nameof(DetalleShaper), new { id });
    }

    [Authorize(Roles = "Shaper")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DefinirPrecio(int id, decimal precio)
    {
        var resultado = _servicio.DefinirPrecio(id, ObtenerUsuarioId(), precio);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Exito
            ? $"Precio final guardado: USD {precio:N2}."
            : resultado.Error;
        return RedirectToAction(nameof(DetalleShaper), new { id });
    }

    private int ObtenerUsuarioId()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
            throw new InvalidOperationException("No se pudo identificar al usuario.");
        return id;
    }
}
