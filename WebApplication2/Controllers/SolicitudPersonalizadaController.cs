using ClassLibrary.Servicios;
using ClassLibrary.Solicitudes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

public class SolicitudPersonalizadaController : Controller
{
    private readonly ISolicitudPersonalizadaServicio _servicio;
    private readonly WebApplication2.Servicios.ICorreoNotificacionServicio? _correo;
    public SolicitudPersonalizadaController(ISolicitudPersonalizadaServicio servicio, WebApplication2.Servicios.ICorreoNotificacionServicio? correo=null){_servicio=servicio;_correo=correo;}

    [Authorize(Roles = "Cliente")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear([FromForm] SolicitudPersonalizada solicitud)
    {
        var resultado = _servicio.Crear(ObtenerUsuarioId(), solicitud);
        if(resultado.Exito&&_correo!=null) _correo.EnviarAUsuarioAsync(solicitud.ShaperId,$"Nueva solicitud personalizada #{resultado.Id}","Recibiste una solicitud personalizada",$"Un cliente envió una nueva configuración para {solicitud.Modelo}. Revisala desde Pedidos recibidos.").GetAwaiter().GetResult();
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
    public async Task<IActionResult> CambiarEstado(int id, byte estado)
    {
        bool actualizado = _servicio.CambiarEstado(id, ObtenerUsuarioId(), estado);
        var detalle=actualizado?_servicio.ObtenerDetalleParaShaper(id,ObtenerUsuarioId()):null;
        if(detalle!=null&&_correo!=null) await _correo.EnviarAUsuarioAsync(detalle.ClienteId,$"Actualización del pedido personalizado #{id}","El shaper actualizó tu solicitud",estado==2?"El shaper indicó que esta configuración no está disponible.":"Tu solicitud fue revisada por el shaper.");
        TempData[actualizado ? "Mensaje" : "Error"] = actualizado
            ? (estado == 1 ? "Pedido personalizado marcado como revisado." : estado == 2
                ? "Pedido personalizado marcado como no disponible." : "Estado actualizado.")
            : "No se pudo actualizar el pedido personalizado.";
        return RedirectToAction(nameof(DetalleShaper), new { id });
    }

    [Authorize(Roles = "Shaper")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DefinirPrecio(int id, decimal precio)
    {
        var resultado = _servicio.DefinirPrecio(id, ObtenerUsuarioId(), precio);
        var detalle=resultado.Exito?_servicio.ObtenerDetalleParaShaper(id,ObtenerUsuarioId()):null;
        if(detalle!=null&&_correo!=null) await _correo.EnviarAUsuarioAsync(detalle.ClienteId,$"Cotización disponible para el pedido #{id}","Tu tabla personalizada ya tiene precio",$"{detalle.ShaperNombre} cotizó tu solicitud en USD {precio:N2}. Ingresá para aceptarla o rechazarla.");
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

    [Authorize(Roles = "Shaper")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarSeguimiento(int id, byte estado, DateTime entrega, string mensaje)
    {
        var resultado=ModelState.IsValid
            ? _servicio.ActualizarSeguimiento(id,ObtenerUsuarioId(),estado,entrega,mensaje)
            : (Exito:false,Error:"Revisá la fecha y los datos del seguimiento.");
        var detalle=resultado.Exito?_servicio.ObtenerDetalleParaShaper(id,ObtenerUsuarioId()):null;
        if(detalle!=null&&_correo!=null) await _correo.EnviarAUsuarioAsync(detalle.ClienteId,$"Novedades del pedido personalizado #{id}","El shaper actualizó el seguimiento",string.IsNullOrWhiteSpace(mensaje)?$"Nuevo estado: {detalle.EstadoNombre}.":mensaje);
        TempData[resultado.Exito?"Mensaje":"Error"]=resultado.Exito?"Seguimiento actualizado. El cliente ya puede verlo en Mis pedidos.":resultado.Error;
        return RedirectToAction(nameof(DetalleShaper),new{id});
    }
}
