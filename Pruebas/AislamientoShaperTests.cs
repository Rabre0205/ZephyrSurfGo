using ClassLibrary.Enums;
using ClassLibrary.Pedidos;
using ClassLibrary.Productos;
using ClassLibrary.Servicios;
using ClassLibrary.Solicitudes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Controllers;
using WebApplication2.Models.Dashboard;

namespace Pruebas;

public class AislamientoShaperTests
{
    [Fact]
    public void DetallePedidoConsultaConElShaperAutenticado()
    {
        var pedidos = new PedidoServicioEspia();
        var controlador = CrearControlador(pedidos, new ProductoServicioEspia(), 81);

        var resultado = controlador.DetallePedido(150);

        Assert.IsType<NotFoundResult>(resultado);
        Assert.Equal(150, pedidos.UltimoPedidoConsultado);
        Assert.Equal(81, pedidos.UltimoShaperConsultado);
    }

    [Fact]
    public void DashboardConsultaProductosYPedidosDelShaperAutenticado()
    {
        var pedidos = new PedidoServicioEspia();
        var productos = new ProductoServicioEspia();
        var controlador = CrearControlador(pedidos, productos, 93);

        var resultado = Assert.IsType<ViewResult>(controlador.Index());
        var modelo = Assert.IsType<DashboardShaperViewModel>(resultado.Model);

        Assert.Equal(93, pedidos.UltimoShaperConsultado);
        Assert.Equal(93, productos.UltimoShaperConsultado);
        Assert.Equal(0, modelo.TotalPedidos);
    }

    private static DashboardController CrearControlador(
        IPedidoServicio pedidos,
        IProductoServicio productos,
        int shaperId)
    {
        var contexto = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, shaperId.ToString()) },
                "Prueba"))
        };

        return new DashboardController(pedidos, productos, new SolicitudServicioEspia())
        {
            ControllerContext = new ControllerContext { HttpContext = contexto }
        };
    }

    private sealed class SolicitudServicioEspia : ISolicitudPersonalizadaServicio
    {
        public (bool Exito, string Error, int Id) Crear(int clienteId, SolicitudPersonalizada solicitud) => (true, string.Empty, 1);
        public List<SolicitudPersonalizada> ObtenerPorShaper(int shaperId) => new();
        public List<SolicitudPersonalizada> ObtenerPorCliente(int clienteId) => new();
        public SolicitudPersonalizada? ObtenerDetalleParaShaper(int id, int shaperId) => null;
        public SolicitudPersonalizada? ObtenerDetalleParaCliente(int id, int clienteId) => null;
        public bool CambiarEstado(int id, int shaperId, byte estado) => true;
        public (bool Exito, string Error) DefinirPrecio(int id, int shaperId, decimal precio) => (true, string.Empty);
        public (bool Exito, string Error) ResponderCotizacion(int id, int clienteId, bool aceptar) => (true, string.Empty);
    }

    private sealed class PedidoServicioEspia : IPedidoServicio
    {
        public int UltimoShaperConsultado { get; private set; }
        public int UltimoPedidoConsultado { get; private set; }

        public PedidoAdminDetalle ObtenerDetalleShaper(int pedidoId, int shaperId)
        {
            UltimoPedidoConsultado = pedidoId;
            UltimoShaperConsultado = shaperId;
            return null!;
        }

        public (int TotalPedidos, int PedidosPendientes, decimal VentasConfirmadas, decimal Comisiones)
            ObtenerResumenShaper(int shaperId)
        {
            UltimoShaperConsultado = shaperId;
            return (0, 0, 0, 0);
        }

        public List<PedidoAdminItem> ObtenerPedidosShaper(
            int shaperId, string busqueda, byte? estadoId,
            int pagina, int cantidadPorPagina)
        {
            UltimoShaperConsultado = shaperId;
            return new();
        }

        public Task<List<(Pedido Pedido, string UrlPago)>> CrearPedidosDesdeCarritoAsync(int clienteId) => Task.FromResult(new List<(Pedido, string)>());
        public Task ProcesarNotificacionPagoAsync(string mercadoPagoPaymentId) => Task.CompletedTask;
        public int ContarPedidos() => 0;
        public int ContarPedidos(string busqueda, byte? estadoId) => 0;
        public List<PedidoAdminItem> ObtenerPedidosAdministracion(int pagina, int cantidadPorPagina) => new();
        public List<PedidoAdminItem> ObtenerPedidosAdministracion(string busqueda, byte? estadoId, int pagina, int cantidadPorPagina) => new();
        public PedidoAdminDetalle ObtenerDetalleAdministracion(int pedidoId) => null!;
        public int ContarPedidosShaper(int shaperId, string busqueda, byte? estadoId) { UltimoShaperConsultado = shaperId; return 0; }
        public (int TotalPedidos, decimal VentasTotales, decimal ComisionTotal) ObtenerResumenAdministracion() => (0, 0, 0);
        public int ContarPedidosPorEstado(byte estadoId) => 0;
    }

    private sealed class ProductoServicioEspia : IProductoServicio
    {
        public int UltimoShaperConsultado { get; private set; }

        public List<Producto> BuscarPorShaper(int shaperId)
        {
            UltimoShaperConsultado = shaperId;
            return new();
        }

        public List<Tabla> ObtenerTablasDelShaper(int shaperId) { UltimoShaperConsultado = shaperId; return new(); }
        public int ContarProductosPublicados() => 0;
        public int ContarProductosAdministracion(string busqueda, string tipo, string estado) => 0;
        public List<ProductoAdminItem> ObtenerProductosAdministracion(string busqueda, string tipo, string estado, int pagina, int cantidadPorPagina) => new();
        public bool CambiarEstadoProducto(int id, bool oculto) => false;
        public int AgregarTabla(string titulo, string subtitulo, double precio, string descripcion, int shaperId, string altura, int ancho, double volumen, SistemaDeEncaje sistemaDeEncaje, TipoDeOla tipoDeOla, EstiloDeSurf estiloDeSurf, int pesoMinimo, int pesoMaximo, Experiencia experiencia, IFormFile imagenFrontal, IFormFile? imagenTrasera) => 0;
    }
}
