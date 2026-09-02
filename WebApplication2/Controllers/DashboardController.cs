using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models.Dashboard;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Shaper")]
    public class DashboardController : Controller
    {
        private readonly IPedidoServicio _pedidoServicio;
        private readonly IProductoServicio _productoServicio;
        private readonly ISolicitudPersonalizadaServicio _personalizados;

        public DashboardController(
            IPedidoServicio pedidoServicio,
            IProductoServicio productoServicio,
            ISolicitudPersonalizadaServicio personalizados)
        {
            _pedidoServicio = pedidoServicio;
            _productoServicio = productoServicio;
            _personalizados = personalizados;
        }

        public IActionResult Index()
        {
            int shaperId = ObtenerUsuarioId();
            var resumen = _pedidoServicio.ObtenerResumenShaper(shaperId);
            var modelo = new DashboardShaperViewModel
            {
                TotalPedidos = resumen.TotalPedidos,
                PedidosPendientes = resumen.PedidosPendientes,
                VentasConfirmadas = resumen.VentasConfirmadas,
                Comisiones = resumen.Comisiones,
                ProductosPublicados = _productoServicio.BuscarPorShaper(shaperId).Count,
                PedidosRecientes = _pedidoServicio.ObtenerPedidosShaper(
                    shaperId, string.Empty, null, 1, 5)
            };
            return View(modelo);
        }

        public IActionResult Pedidos(
            string busqueda = "", byte? estadoId = null, int pagina = 1)
        {
            const int cantidadPorPagina = 20;
            int shaperId = ObtenerUsuarioId();
            busqueda = busqueda?.Trim() ?? string.Empty;
            pagina = Math.Max(1, pagina);
            int total = _pedidoServicio.ContarPedidosShaper(shaperId, busqueda, estadoId);
            var personalizados = _personalizados.ObtenerPorShaper(shaperId);
            int paginas = (int)Math.Ceiling(total / (double)cantidadPorPagina);
            if (paginas > 0 && pagina > paginas) pagina = paginas;

            return View(new PedidosShaperViewModel
            {
                Pedidos = _pedidoServicio.ObtenerPedidosShaper(
                    shaperId, busqueda, estadoId, pagina, cantidadPorPagina),
                Personalizados = personalizados,
                Busqueda = busqueda,
                EstadoId = estadoId,
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalResultados = total + personalizados.Count
            });
        }

        public IActionResult DetallePedido(int id)
        {
            var pedido = _pedidoServicio.ObtenerDetalleShaper(id, ObtenerUsuarioId());
            return pedido == null ? NotFound() : View(pedido);
        }

        public IActionResult Facturacion()
        {
            int shaperId = ObtenerUsuarioId();
            var resumen = _pedidoServicio.ObtenerResumenShaper(shaperId);
            var aprobados = _pedidoServicio.ObtenerPedidosShaper(
                shaperId, string.Empty, 1, 1, 50);
            var completados = _pedidoServicio.ObtenerPedidosShaper(
                shaperId, string.Empty, 4, 1, 50);

            return View(new FacturacionShaperViewModel
            {
                VentasConfirmadas = resumen.VentasConfirmadas,
                Comisiones = resumen.Comisiones,
                Movimientos = aprobados
                    .Concat(completados)
                    .OrderByDescending(pedido => pedido.FechaCreacion)
                    .Take(50)
                    .ToList()
            });
        }

        private int ObtenerUsuarioId()
        {
            string? valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(valor, out int id))
                throw new InvalidOperationException("No se pudo identificar al shaper autenticado.");
            return id;
        }
    }
}
