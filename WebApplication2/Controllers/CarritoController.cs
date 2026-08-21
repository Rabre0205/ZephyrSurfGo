using ClassLibrary.Datos;
using ClassLibrary.Pedidos;
using ClassLibrary.Servicios;
using MercadoPago.Resource.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Cliente")]
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
            int clienteId = ObtenerClienteId();
            var items = _carritoRepositorio.ObtenerPorUsuario(clienteId);
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(int productoId, int cantidad = 1)
        {
            int clienteId = ObtenerClienteId();

            if (cantidad < 1)
                return ResponderErrorAgregar("La cantidad debe ser mayor que cero.", clienteId);

            var itemsActuales = _carritoRepositorio.ObtenerPorUsuario(clienteId);
            var itemExistente = itemsActuales.FirstOrDefault(item => item.ProductoId == productoId);
            var disponibilidad = _carritoRepositorio.ObtenerDisponibilidad(productoId);

            if (disponibilidad == null || !disponibilidad.Value.Disponible)
                return ResponderErrorAgregar("El producto ya no está disponible.", clienteId);

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

            int cantidadFinal = (itemExistente?.Cantidad ?? 0) + cantidad;
            if (disponibilidad.Value.Stock.HasValue &&
                cantidadFinal > disponibilidad.Value.Stock.Value)
            {
                return ResponderErrorAgregar(
                    $"Solo quedan {disponibilidad.Value.Stock.Value} unidades disponibles.", clienteId);
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
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarCantidad(int productoId, int cantidad)
        {
            int clienteId = ObtenerClienteId();
            var item = _carritoRepositorio.ObtenerPorUsuario(clienteId)
                .FirstOrDefault(actual => actual.ProductoId == productoId);

            if (item == null)
            {
                TempData["Error"] = "El producto no se encuentra en tu carrito.";
                return RedirectToAction(nameof(Index));
            }

            if (cantidad <= 0)
            {
                _carritoRepositorio.EliminarItem(clienteId, productoId);
                TempData["Mensaje"] = "Producto quitado del carrito.";
                return RedirectToAction(nameof(Index));
            }

            if (item.TipoProducto == "Tabla" && cantidad != 1)
            {
                TempData["Error"] = "Las tablas únicas deben mantenerse en cantidad 1.";
                return RedirectToAction(nameof(Index));
            }

            var disponibilidad = _carritoRepositorio.ObtenerDisponibilidad(productoId);
            if (disponibilidad == null || !disponibilidad.Value.Disponible ||
                (disponibilidad.Value.Stock.HasValue && cantidad > disponibilidad.Value.Stock.Value))
            {
                TempData["Error"] = disponibilidad?.Stock is int stock
                    ? $"Solo quedan {stock} unidades disponibles."
                    : "El producto ya no está disponible.";
                return RedirectToAction(nameof(Index));
            }

            if (!_carritoRepositorio.ActualizarCantidad(clienteId, productoId, cantidad))
            {
                TempData["Error"] = "No se pudo actualizar la cantidad.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = "Cantidad actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int productoId)
        {
            int clienteId = ObtenerClienteId();
            _carritoRepositorio.EliminarItem(clienteId, productoId);
            TempData["Mensaje"] = "Producto quitado del carrito.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            int clienteId = ObtenerClienteId();

            if (_carritoRepositorio.ObtenerPorUsuario(clienteId).Count == 0)
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

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

        private IActionResult ResponderErrorAgregar(string mensaje, int clienteId)
        {
            var items = _carritoRepositorio.ObtenerPorUsuario(clienteId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    agregado = false,
                    mensaje,
                    cantidadCarrito = items.Sum(item => item.Cantidad)
                });
            }

            TempData["Error"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        private int ObtenerClienteId()
        {
            string? valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(valor, out int clienteId))
                throw new InvalidOperationException("No se pudo identificar al cliente autenticado.");
            return clienteId;
        }
    }
}
