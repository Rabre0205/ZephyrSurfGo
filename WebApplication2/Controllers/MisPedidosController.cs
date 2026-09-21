using ClassLibrary.Datos;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles = "Cliente")]
public class MisPedidosController : Controller
{
    private readonly IPedidoRepositorio _pedidos;
    private readonly ISolicitudPersonalizadaServicio _personalizados;
    private readonly WebApplication2.Servicios.ICorreoNotificacionServicio? _correo;

    public MisPedidosController(
        IPedidoRepositorio pedidos,
        ISolicitudPersonalizadaServicio personalizados, WebApplication2.Servicios.ICorreoNotificacionServicio? correo=null)
    {
        _pedidos = pedidos;
        _personalizados = personalizados;
        _correo=correo;
    }

    public IActionResult Index()
    {
        int clienteId = ObtenerClienteId();
        return View(new Models.MisPedidos.MisPedidosViewModel
        {
            Compras = _pedidos.ObtenerPedidosCliente(clienteId),
            Personalizados = _personalizados.ObtenerPorCliente(clienteId)
        });
    }

    public IActionResult Detalle(int id)
    {
        var pedido = _pedidos.ObtenerDetalleCliente(id, ObtenerClienteId());
        return pedido == null ? NotFound() : View(pedido);
    }

    public IActionResult DetallePersonalizado(int id)
    {
        var pedido = _personalizados.ObtenerDetalleParaCliente(id, ObtenerClienteId());
        return pedido == null
            ? NotFound()
            : View("~/Views/SolicitudPersonalizada/Detalle.cshtml", pedido);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResponderCotizacion(int id, bool aceptar)
    {
        var resultado = _personalizados.ResponderCotizacion(
            id, ObtenerClienteId(), aceptar);
        var detalle=resultado.Exito?_personalizados.ObtenerDetalleParaCliente(id,ObtenerClienteId()):null;
        if(detalle!=null&&_correo!=null) await _correo.EnviarAUsuarioAsync(detalle.ShaperId,$"Respuesta a la cotización #{id}",aceptar?"El cliente aceptó tu cotización":"El cliente rechazó tu cotización",$"Solicitud para {detalle.Modelo}. Revisá el detalle desde Pedidos recibidos.");
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Exito
            ? (aceptar
                ? "Aceptaste la cotización. El pedido queda preparado para continuar con el pago."
                : "Rechazaste la cotización.")
            : resultado.Error;
        return RedirectToAction(nameof(DetallePersonalizado), new { id });
    }

    private int ObtenerClienteId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id)
            ? id
            : throw new UnauthorizedAccessException("No se pudo identificar al cliente.");
}
