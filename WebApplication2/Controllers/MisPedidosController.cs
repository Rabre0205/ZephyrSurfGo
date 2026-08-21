using ClassLibrary.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles = "Cliente")]
public class MisPedidosController : Controller
{
    private readonly IPedidoRepositorio _pedidos;
    public MisPedidosController(IPedidoRepositorio pedidos) => _pedidos = pedidos;

    public IActionResult Index() => View(_pedidos.ObtenerPedidosCliente(ObtenerClienteId()));

    public IActionResult Detalle(int id)
    {
        var pedido = _pedidos.ObtenerDetalleCliente(id, ObtenerClienteId());
        return pedido == null ? NotFound() : View(pedido);
    }

    private int ObtenerClienteId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id)
            ? id
            : throw new UnauthorizedAccessException("No se pudo identificar al cliente.");
}
