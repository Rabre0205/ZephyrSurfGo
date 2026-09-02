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

    public MisPedidosController(
        IPedidoRepositorio pedidos,
        ISolicitudPersonalizadaServicio personalizados)
    {
        _pedidos = pedidos;
        _personalizados = personalizados;
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

    private int ObtenerClienteId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id)
            ? id
            : throw new UnauthorizedAccessException("No se pudo identificar al cliente.");
}
