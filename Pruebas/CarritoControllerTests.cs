using ClassLibrary.Carrito;
using ClassLibrary.Datos;
using ClassLibrary.Pedidos;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Data.SqlClient;
using System.Security.Claims;
using WebApplication2.Controllers;

namespace Pruebas;

public class CarritoControllerTests
{
    [Fact]
    public void AgregarNoDuplicaUnaTablaQueYaEstaEnElCarrito()
    {
        var repositorio = new CarritoRepositorioFalso();
        repositorio.DefinirProducto(10, "Tabla", null, disponible: true);
        repositorio.AgregarItem(42, 10, 1);
        repositorio.ReiniciarContador();
        var controlador = CrearControlador(repositorio, usuarioId: 42, ajax: true);

        var resultado = Assert.IsType<JsonResult>(controlador.Agregar(10));

        Assert.False(LeerPropiedad<bool>(resultado.Value!, "agregado"));
        Assert.Equal(1, LeerPropiedad<int>(resultado.Value!, "cantidadCarrito"));
        Assert.Equal(0, repositorio.LlamadasAgregar);
        Assert.Equal(1, repositorio.ObtenerPorUsuario(42).Single().Cantidad);
    }

    [Fact]
    public void AgregarSumaCantidadParaUnAccesorio()
    {
        var repositorio = new CarritoRepositorioFalso();
        repositorio.DefinirProducto(20, "Quilla", stock: 5, disponible: true);
        repositorio.AgregarItem(42, 20, 1);
        repositorio.ReiniciarContador();
        var controlador = CrearControlador(repositorio, usuarioId: 42, ajax: true);

        var resultado = Assert.IsType<JsonResult>(controlador.Agregar(20, 2));

        Assert.True(LeerPropiedad<bool>(resultado.Value!, "agregado"));
        Assert.Equal(3, LeerPropiedad<int>(resultado.Value!, "cantidadCarrito"));
        Assert.Equal(1, repositorio.LlamadasAgregar);
        Assert.Equal(3, repositorio.ObtenerPorUsuario(42).Single().Cantidad);
    }

    [Fact]
    public void AgregarRechazaCantidadMayorAlStock()
    {
        var repositorio = new CarritoRepositorioFalso();
        repositorio.DefinirProducto(30, "Pad", stock: 2, disponible: true);
        var controlador = CrearControlador(repositorio, usuarioId: 42, ajax: true);

        var resultado = Assert.IsType<JsonResult>(controlador.Agregar(30, 3));

        Assert.False(LeerPropiedad<bool>(resultado.Value!, "agregado"));
        Assert.Equal(0, repositorio.LlamadasAgregar);
        Assert.Empty(repositorio.ObtenerPorUsuario(42));
    }

    [Fact]
    public void OperacionesDelCarritoUsanElUsuarioAutenticado()
    {
        var repositorio = new CarritoRepositorioFalso();
        repositorio.DefinirProducto(40, "Leash", stock: 10, disponible: true);
        var controlador = CrearControlador(repositorio, usuarioId: 73, ajax: true);

        controlador.Agregar(40, 1);

        Assert.Equal(73, repositorio.UltimoUsuarioConsultado);
        Assert.Single(repositorio.ObtenerPorUsuario(73));
        Assert.Empty(repositorio.ObtenerPorUsuario(42));
    }

    [Fact]
    public void ActualizarConCeroEliminaElProducto()
    {
        var repositorio = new CarritoRepositorioFalso();
        repositorio.DefinirProducto(50, "Traje", stock: 4, disponible: true);
        repositorio.AgregarItem(42, 50, 2);
        var controlador = CrearControlador(repositorio, usuarioId: 42);

        var resultado = Assert.IsType<RedirectToActionResult>(
            controlador.ActualizarCantidad(50, 0));

        Assert.Equal(nameof(CarritoController.Index), resultado.ActionName);
        Assert.Empty(repositorio.ObtenerPorUsuario(42));
    }

    private static CarritoController CrearControlador(
        CarritoRepositorioFalso repositorio,
        int usuarioId,
        bool ajax = false)
    {
        var contexto = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()) },
                "Prueba"))
        };

        if (ajax)
            contexto.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        return new CarritoController(repositorio, new PedidoServicioFalso())
        {
            ControllerContext = new ControllerContext { HttpContext = contexto },
            TempData = new TempDataDictionary(contexto, new TempDataProviderFalso())
        };
    }

    private static T LeerPropiedad<T>(object objeto, string nombre) =>
        (T)objeto.GetType().GetProperty(nombre)!.GetValue(objeto)!;

    private sealed class TempDataProviderFalso : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(
            HttpContext context,
            IDictionary<string, object> values)
        {
        }
    }

    private sealed class CarritoRepositorioFalso : ICarritoRepositorio
    {
        private readonly Dictionary<int, List<CarritoItemDetallado>> _carritos = new();
        private readonly Dictionary<int, (string Tipo, int? Stock, bool Disponible)> _productos = new();

        public int LlamadasAgregar { get; private set; }
        public int UltimoUsuarioConsultado { get; private set; }

        public void DefinirProducto(int id, string tipo, int? stock, bool disponible) =>
            _productos[id] = (tipo, stock, disponible);

        public void ReiniciarContador() => LlamadasAgregar = 0;

        public List<CarritoItemDetallado> ObtenerPorUsuario(int usuarioId)
        {
            UltimoUsuarioConsultado = usuarioId;
            return _carritos.TryGetValue(usuarioId, out var items)
                ? items.Select(Copiar).ToList()
                : new List<CarritoItemDetallado>();
        }

        public void AgregarItem(int usuarioId, int productoId, int cantidad)
        {
            LlamadasAgregar++;
            if (!_carritos.TryGetValue(usuarioId, out var items))
            {
                items = new List<CarritoItemDetallado>();
                _carritos[usuarioId] = items;
            }

            var producto = _productos[productoId];
            var existente = items.SingleOrDefault(item => item.ProductoId == productoId);
            if (existente == null)
            {
                items.Add(new CarritoItemDetallado
                {
                    ProductoId = productoId,
                    TipoProducto = producto.Tipo,
                    Titulo = $"Producto {productoId}",
                    Cantidad = cantidad,
                    StockDisponible = producto.Stock,
                    Disponible = producto.Disponible
                });
            }
            else
            {
                existente.Cantidad = producto.Tipo == "Tabla"
                    ? 1
                    : existente.Cantidad + cantidad;
            }
        }

        public bool ActualizarCantidad(int usuarioId, int productoId, int cantidad)
        {
            var item = _carritos.GetValueOrDefault(usuarioId)?
                .SingleOrDefault(actual => actual.ProductoId == productoId);
            if (item == null) return false;
            item.Cantidad = cantidad;
            return true;
        }

        public (string TipoProducto, int? Stock, bool Disponible)?
            ObtenerDisponibilidad(int productoId) =>
            _productos.TryGetValue(productoId, out var producto)
                ? (producto.Tipo, producto.Stock, producto.Disponible)
                : null;

        public void EliminarItem(int usuarioId, int productoId) =>
            _carritos.GetValueOrDefault(usuarioId)?
                .RemoveAll(item => item.ProductoId == productoId);

        public void EliminarItem(
            int usuarioId,
            int productoId,
            SqlConnection conexion,
            SqlTransaction transaccion) =>
            EliminarItem(usuarioId, productoId);

        private static CarritoItemDetallado Copiar(CarritoItemDetallado item) => new()
        {
            ProductoId = item.ProductoId,
            TipoProducto = item.TipoProducto,
            Titulo = item.Titulo,
            Cantidad = item.Cantidad,
            StockDisponible = item.StockDisponible,
            Disponible = item.Disponible
        };
    }

    private sealed class PedidoServicioFalso : IPedidoServicio
    {
        public Task<List<(Pedido Pedido, string UrlPago)>>
            CrearPedidosDesdeCarritoAsync(int clienteId) =>
            Task.FromResult(new List<(Pedido, string)>());

        public Task ProcesarNotificacionPagoAsync(string mercadoPagoPaymentId) =>
            Task.CompletedTask;

        public int ContarPedidos() => 0;
        public int ContarPedidos(string busqueda, byte? estadoId) => 0;
        public List<PedidoAdminItem> ObtenerPedidosAdministracion(int pagina, int cantidadPorPagina) => new();
        public List<PedidoAdminItem> ObtenerPedidosAdministracion(string busqueda, byte? estadoId, int pagina, int cantidadPorPagina) => new();
        public PedidoAdminDetalle ObtenerDetalleAdministracion(int pedidoId) => null!;
        public int ContarPedidosShaper(int shaperId, string busqueda, byte? estadoId) => 0;
        public List<PedidoAdminItem> ObtenerPedidosShaper(int shaperId, string busqueda, byte? estadoId, int pagina, int cantidadPorPagina) => new();
        public PedidoAdminDetalle ObtenerDetalleShaper(int pedidoId, int shaperId) => null!;
        public (int TotalPedidos, int PedidosPendientes, decimal VentasConfirmadas, decimal Comisiones) ObtenerResumenShaper(int shaperId) => (0, 0, 0, 0);
        public (int TotalPedidos, decimal VentasTotales, decimal ComisionTotal) ObtenerResumenAdministracion() => (0, 0, 0);
        public int ContarPedidosPorEstado(byte estadoId) => 0;
    }
}
