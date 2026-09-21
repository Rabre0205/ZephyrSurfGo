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
            string busqueda = "", string tipo = "todos", string estado = "", int pagina = 1)
        {
            const int cantidadPorPagina = 20;
            int shaperId = ObtenerUsuarioId();
            busqueda = busqueda?.Trim() ?? string.Empty;
            tipo = (tipo ?? "todos").Trim().ToLowerInvariant();
            if (tipo is not ("todos" or "compras" or "personalizados")) tipo = "todos";
            estado = (estado ?? string.Empty).Trim().ToLowerInvariant();
            pagina = Math.Max(1, pagina);

            byte? estadoCompra = null;
            byte? estadoPersonalizado = null;
            if (estado.StartsWith("compra:") && byte.TryParse(estado[7..], out byte compra))
                estadoCompra = compra;
            else if (estado.StartsWith("personalizado:") && byte.TryParse(estado[15..], out byte personalizado))
                estadoPersonalizado = personalizado;
            else if (!string.IsNullOrEmpty(estado))
                estado = string.Empty;

            bool mostrarCompras = tipo != "personalizados" && !estadoPersonalizado.HasValue;
            bool mostrarPersonalizados = tipo != "compras" && !estadoCompra.HasValue;

            int totalCompras = mostrarCompras
                ? _pedidoServicio.ContarPedidosShaper(shaperId, busqueda, estadoCompra)
                : 0;

            var personalizados = mostrarPersonalizados
                ? _personalizados.ObtenerPorShaper(shaperId)
                    .Where(p => !estadoPersonalizado.HasValue || p.Estado == estadoPersonalizado.Value)
                    .Where(p => CoincidePedidoPersonalizado(p, busqueda))
                    .ToList()
                : new List<ClassLibrary.Solicitudes.SolicitudPersonalizada>();

            int paginas = (int)Math.Ceiling(totalCompras / (double)cantidadPorPagina);
            if (paginas > 0 && pagina > paginas) pagina = paginas;

            return View(new PedidosShaperViewModel
            {
                Pedidos = mostrarCompras
                    ? _pedidoServicio.ObtenerPedidosShaper(
                        shaperId, busqueda, estadoCompra, pagina, cantidadPorPagina)
                    : new(),
                Personalizados = personalizados,
                Busqueda = busqueda,
                EstadoId = estadoCompra,
                Tipo = tipo,
                EstadoFiltro = estado,
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalResultados = totalCompras + personalizados.Count
            });
        }

        private static bool CoincidePedidoPersonalizado(
            ClassLibrary.Solicitudes.SolicitudPersonalizada pedido, string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda)) return true;
            var comparacion = StringComparison.OrdinalIgnoreCase;
            return pedido.Id.ToString() == busqueda
                || pedido.ClienteNombre.Contains(busqueda, comparacion)
                || pedido.ClienteEmail.Contains(busqueda, comparacion)
                || pedido.Modelo.Contains(busqueda, comparacion);
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
