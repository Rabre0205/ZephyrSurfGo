using ClassLibrary.Datos;
using ClassLibrary.Pedidos;
using ClassLibrary.Servicios;
using MercadoPago.Resource.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class CarritoController : Controller
    {
        private readonly ICarritoRepositorio _carritoRepositorio;
        private readonly IPedidoServicio _pedidoServicio;

        public CarritoController(ICarritoRepositorio carritoRepositorio, IPedidoServicio pedidoServicio)
        {
            _carritoRepositorio = carritoRepositorio;
            _pedidoServicio = pedidoServicio;
        }
        public IActionResult Index()
        {
            int clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var items = _carritoRepositorio.ObtenerPorUsuario(clienteId);
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(int productoId, int cantidad = 1)
        {
            int clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var itemsActuales = _carritoRepositorio.ObtenerPorUsuario(clienteId);
            var itemExistente = itemsActuales.FirstOrDefault(item => item.ProductoId == productoId);

            if (itemExistente?.TipoProducto == "Tabla")
            {
                const string mensaje = "Esta tabla ya está en tu carrito. Las tablas únicas solo pueden agregarse una vez.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        agregado = false,
                        mensaje,
                        cantidadCarrito = itemsActuales.Sum(item => item.Cantidad)
                    });
                }

                TempData["Mensaje"] = mensaje;
                return RedirectToAction("Index");
            }

            _carritoRepositorio.AgregarItem(clienteId, productoId, cantidad);

            var itemsActualizados = _carritoRepositorio.ObtenerPorUsuario(clienteId);
            const string mensajeAgregado = "Producto agregado al carrito.";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    agregado = true,
                    mensaje = mensajeAgregado,
                    cantidadCarrito = itemsActualizados.Sum(item => item.Cantidad)
                });
            }

            TempData["Mensaje"] = mensajeAgregado;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int productoId)
        {
            int clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _carritoRepositorio.EliminarItem(clienteId, productoId);
            TempData["Mensaje"] = "Producto quitado del carrito.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            int clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<(Pedido Pedido, string UrlPago)> pedidos;
            try
            {
                pedidos = await _pedidoServicio.CrearPedidosDesdeCarritoAsync(clienteId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Carrito");
            }

            if (pedidos.Count == 1)
                return Redirect(pedidos[0].UrlPago);

            // Varios shapers en el carrito: no se puede redirigir a 2 lugares
            // a la vez, así que mostramos un botón "Pagar" por cada pedido generado.
            var pedidosParaVista = new List<Pedido>();
            foreach (var (pedido, urlPago) in pedidos)
            {
                pedido.MercadoPagoPreferenceId = urlPago; // reuso el campo para pasar la URL a la vista
                pedidosParaVista.Add(pedido);
            }

            return View("ConfirmarPagos", pedidosParaVista);
        }
    }
}
