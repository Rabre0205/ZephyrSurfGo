using ClassLibrary.Persona;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models.PanelAdmin;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PanelAdminController : Controller
    {
        private readonly IUsuarioServicio _usuarioServicio;

        private readonly IProductoServicio _productoServicio;

        private readonly IPedidoServicio _pedidoServicio;

        public PanelAdminController(
    IUsuarioServicio usuarioServicio,
    IProductoServicio productoServicio,
    IPedidoServicio pedidoServicio)
        {
            _usuarioServicio = usuarioServicio;
            _productoServicio = productoServicio;
            _pedidoServicio = pedidoServicio;
        }

        public IActionResult Index()
        {
            var resumenPedidos =
                _pedidoServicio.ObtenerResumenAdministracion();

            var modelo = new DashboardAdminViewModel
            {
                TotalShapers =
                    _usuarioServicio.ContarShapers(
                        string.Empty
                    ),

                ShapersActivos =
                    _usuarioServicio.ContarShapersActivos(),

                TotalClientes =
                    _usuarioServicio.ContarClientes(),

                TotalProductos =
                    _productoServicio.ContarProductosPublicados(),

                TotalPedidos =
                    resumenPedidos.TotalPedidos,

                VentasTotales =
                    resumenPedidos.VentasTotales,

                ComisionTotal =
                    resumenPedidos.ComisionTotal
            };

            return View(modelo);
        }

        public IActionResult Shapers(
    string busqueda = "",
    int pagina = 1)
        {
            const int cantidadPorPagina = 20;

            if (pagina < 1)
            {
                pagina = 1;
            }

            string textoBusqueda =
                busqueda?.Trim() ?? string.Empty;

            int totalResultados =
                _usuarioServicio.ContarShapers(
                    textoBusqueda
                );

            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)cantidadPorPagina
                );

            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            var shapers =
                _usuarioServicio.ObtenerShapersPaginados(
                    textoBusqueda,
                    pagina,
                    cantidadPorPagina
                );

            var modelo =
                new ShapersAdminViewModel
                {
                    Shapers = shapers,
                    Busqueda = textoBusqueda,
                    PaginaActual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalResultados = totalResultados
                };

            return View(modelo);
        }

        [HttpGet]
        public IActionResult EditarShaper(int id)
        {
            Shaper shaper = _usuarioServicio.ObtenerShaperPorId(id);

            if (shaper == null)
            {
                return NotFound();
            }

            EditarShaperViewModel modelo = new EditarShaperViewModel
            {
                Id = shaper.Id,
                Nombre = shaper.Nombre,
                Email = shaper.Email,
                Pais = shaper.Pais,
                NombreDeNegosio = shaper.NombreDeNegosio,
                Contacto = shaper.Contacto
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarShaper(EditarShaperViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = _usuarioServicio.ActualizarShaper(
                modelo.Id,
                modelo.Email,
                modelo.Nombre,
                modelo.Pais,
                modelo.NombreDeNegosio,
                modelo.Contacto
            );

            if (!resultado.Exito)
            {
                ModelState.AddModelError(
                    string.Empty,
                    resultado.Error
                );

                return View(modelo);
            }

            TempData["Mensaje"] =
                "Shaper actualizado correctamente.";

            return RedirectToAction(nameof(Shapers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstadoShaper(int id)
        {
            var shaper = _usuarioServicio.ObtenerShaperPorId(id);

            if (shaper == null)
            {
                TempData["Error"] = "No se encontró el shaper.";
                return RedirectToAction(nameof(Shapers));
            }

            bool nuevoEstado = !shaper.Activo;

            bool actualizado = _usuarioServicio.CambiarEstadoShaper(
                id,
                nuevoEstado
            );

            if (!actualizado)
            {
                TempData["Error"] =
                    "No se pudo modificar el estado del shaper.";
            }
            else
            {
                TempData["Mensaje"] = nuevoEstado
                    ? "El shaper fue activado correctamente."
                    : "El shaper fue desactivado correctamente.";
            }

            return RedirectToAction(nameof(Shapers));
        }

        [HttpGet]
        public IActionResult RegistrarShaper()
        {
            return View(new RegistrarShaperViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarShaper(
            RegistrarShaperViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = _usuarioServicio.RegistrarShaper(
                modelo.Email.Trim(),
                modelo.Nombre.Trim(),
                modelo.Pais,
                modelo.Contrasenia,
                modelo.ConfirmarContrasenia,
                modelo.NombreDeNegosio.Trim(),
                modelo.Contacto.Trim()
            );

            if (!resultado.Exito)
            {
                ModelState.AddModelError(
                    string.Empty,
                    resultado.Error
                );

                return View(modelo);
            }

            TempData["Mensaje"] =
                "El shaper fue registrado correctamente.";

            return RedirectToAction(nameof(Shapers));
        }

        public IActionResult Productos(
            string busqueda = "", string tipo = "",
            string estado = "", int pagina = 1)
        {
            const int cantidadPorPagina = 20;
            busqueda = busqueda?.Trim() ?? string.Empty;
            tipo = tipo?.Trim() ?? string.Empty;
            estado = estado?.Trim().ToLowerInvariant() ?? string.Empty;
            pagina = Math.Max(1, pagina);

            int totalResultados = _productoServicio
                .ContarProductosAdministracion(busqueda, tipo, estado);
            int totalPaginas = (int)Math.Ceiling(
                totalResultados / (double)cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            var modelo = new ProductosAdminViewModel
            {
                Productos = _productoServicio.ObtenerProductosAdministracion(
                    busqueda, tipo, estado, pagina, cantidadPorPagina),
                Busqueda = busqueda,
                Tipo = tipo,
                Estado = estado,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalResultados = totalResultados
            };

            return View(modelo);
        }

        public IActionResult Clientes(string busqueda = "", int pagina = 1)
        {
            const int cantidadPorPagina = 20;
            busqueda = busqueda?.Trim() ?? string.Empty;
            pagina = Math.Max(1, pagina);
            int totalResultados = _usuarioServicio.ContarClientes(busqueda);
            int totalPaginas = (int)Math.Ceiling(totalResultados / (double)cantidadPorPagina);
            if (totalPaginas > 0 && pagina > totalPaginas) pagina = totalPaginas;

            return View(new ClientesAdminViewModel
            {
                Clientes = _usuarioServicio.ObtenerClientesPaginados(
                    busqueda, pagina, cantidadPorPagina),
                Busqueda = busqueda,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalResultados = totalResultados
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstadoCliente(
            int id, bool activar, string busqueda = "", int pagina = 1)
        {
            bool actualizado = _usuarioServicio.CambiarEstadoCliente(id, activar);
            TempData[actualizado ? "Mensaje" : "Error"] = actualizado
                ? (activar ? "El cliente fue activado." : "El cliente fue bloqueado.")
                : "No se pudo modificar el cliente.";
            return RedirectToAction(nameof(Clientes), new { busqueda, pagina });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstadoProducto(
            int id, bool ocultar, string busqueda = "",
            string tipo = "", string estado = "", int pagina = 1)
        {
            bool actualizado = _productoServicio.CambiarEstadoProducto(id, ocultar);
            if (actualizado)
                TempData["Mensaje"] = ocultar
                    ? "El producto fue ocultado correctamente."
                    : "El producto fue publicado nuevamente.";
            else
                TempData["Error"] = "No se pudo modificar el producto.";

            return RedirectToAction(nameof(Productos), new
            {
                busqueda,
                tipo,
                estado,
                pagina
            });
        }

        public IActionResult Pedidos(string busqueda = "", byte? estadoId = null, int pagina = 1)
        {
            const int cantidadPorPagina = 20;

            if (pagina < 1)
            {
                pagina = 1;
            }

            busqueda = busqueda?.Trim() ?? string.Empty;
            int totalResultados = _pedidoServicio.ContarPedidos(busqueda, estadoId);

            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)cantidadPorPagina
                );

            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            var pedidos =
                _pedidoServicio.ObtenerPedidosAdministracion(
                    busqueda, estadoId, pagina, cantidadPorPagina);

            var modelo = new PedidosAdminViewModel
            {
                Pedidos = pedidos,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalResultados = totalResultados,
                Busqueda = busqueda,
                EstadoId = estadoId
            };

            return View(modelo);
        }

        public IActionResult DetallePedido(int id)
        {
            var pedido = _pedidoServicio.ObtenerDetalleAdministracion(id);
            return pedido == null ? NotFound() : View(pedido);
        }

        public IActionResult Estadisticas()
        {
            var resumenPedidos =
                _pedidoServicio.ObtenerResumenAdministracion();

            decimal ticketPromedio = 0;

            if (resumenPedidos.TotalPedidos > 0)
            {
                ticketPromedio =
                    resumenPedidos.VentasTotales /
                    resumenPedidos.TotalPedidos;
            }

            var modelo = new EstadisticasAdminViewModel
            {
                TotalPedidos =
                    resumenPedidos.TotalPedidos,

                VentasTotales =
                    resumenPedidos.VentasTotales,

                ComisionTotal =
                    resumenPedidos.ComisionTotal,

                TicketPromedio =
                    ticketPromedio,

                TotalShapers =
                    _usuarioServicio.ContarShapers(
                        string.Empty
                    ),

                ShapersActivos =
                    _usuarioServicio.ContarShapersActivos(),

                TotalClientes =
                    _usuarioServicio.ContarClientes(),

                TotalProductos =
                    _productoServicio.ContarProductosPublicados(),

                // 0 = Pendiente
                PedidosPendientes =
                    _pedidoServicio.ContarPedidosPorEstado(0),

                // 1 = Aprobado
                PedidosAprobados =
                    _pedidoServicio.ContarPedidosPorEstado(1),

                // 2 = Rechazado
                PedidosRechazados =
                    _pedidoServicio.ContarPedidosPorEstado(2),

                // 3 = Cancelado
                PedidosCancelados =
                    _pedidoServicio.ContarPedidosPorEstado(3),

                // 4 = Completado
                PedidosCompletados =
                    _pedidoServicio.ContarPedidosPorEstado(4)
            };

            return View(modelo);
        }
    }
}
