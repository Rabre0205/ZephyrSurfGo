using ClassLibrary.Enums;
using ClassLibrary.Pedidos;
using ClassLibrary.Persona;
using ClassLibrary.Productos;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WebApplication2.Controllers;
using WebApplication2.Models.PanelAdmin;

namespace Pruebas;

public class PanelAdminControllerTests
{
    [Fact]
    public void ClientesNormalizaBusquedaYPaginaAntesDeConsultar()
    {
        var usuarios = new UsuarioServicioEspia { TotalClientes = 45 };
        var controlador = CrearControlador(usuarios);

        var resultado = Assert.IsType<ViewResult>(
            controlador.Clientes("  ana@ejemplo.com  ", pagina: 99));
        var modelo = Assert.IsType<ClientesAdminViewModel>(resultado.Model);

        Assert.Equal("ana@ejemplo.com", usuarios.UltimaBusquedaClientes);
        Assert.Equal(3, usuarios.UltimaPaginaClientes);
        Assert.Equal(3, modelo.PaginaActual);
        Assert.Equal(3, modelo.TotalPaginas);
    }

    [Fact]
    public void CambiarEstadoClienteEnviaLaDecisionAlServicio()
    {
        var usuarios = new UsuarioServicioEspia { ResultadoCambioCliente = true };
        var controlador = CrearControlador(usuarios);

        var resultado = Assert.IsType<RedirectToActionResult>(
            controlador.CambiarEstadoCliente(17, activar: false));

        Assert.Equal(17, usuarios.UltimoClienteModificado);
        Assert.False(usuarios.UltimoEstadoCliente);
        Assert.Equal(nameof(PanelAdminController.Clientes), resultado.ActionName);
        Assert.Equal("El cliente fue bloqueado.", controlador.TempData["Mensaje"]);
    }

    [Fact]
    public void ProductosNormalizaFiltrosYPagina()
    {
        var productos = new ProductoServicioEspia { TotalProductos = 21 };
        var controlador = CrearControlador(new UsuarioServicioEspia(), productos);

        var resultado = Assert.IsType<ViewResult>(
            controlador.Productos("  tabla ", " Tabla ", " PUBLICADO ", 50));
        var modelo = Assert.IsType<ProductosAdminViewModel>(resultado.Model);

        Assert.Equal("tabla", productos.UltimaBusqueda);
        Assert.Equal("Tabla", productos.UltimoTipo);
        Assert.Equal("publicado", productos.UltimoEstado);
        Assert.Equal(2, productos.UltimaPagina);
        Assert.Equal(2, modelo.TotalPaginas);
    }

    private static PanelAdminController CrearControlador(
        UsuarioServicioEspia usuarios,
        ProductoServicioEspia? productos = null)
    {
        var contexto = new DefaultHttpContext();
        return new PanelAdminController(
            usuarios,
            productos ?? new ProductoServicioEspia(),
            new PedidoServicioVacio())
        {
            ControllerContext = new ControllerContext { HttpContext = contexto },
            TempData = new TempDataDictionary(contexto, new TempDataProviderVacio())
        };
    }

    private sealed class TempDataProviderVacio : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class UsuarioServicioEspia : IUsuarioServicio
    {
        public int TotalClientes { get; set; }
        public bool ResultadoCambioCliente { get; set; }
        public string UltimaBusquedaClientes { get; private set; } = string.Empty;
        public int UltimaPaginaClientes { get; private set; }
        public int UltimoClienteModificado { get; private set; }
        public bool UltimoEstadoCliente { get; private set; }

        public int ContarClientes(string busqueda) { UltimaBusquedaClientes = busqueda; return TotalClientes; }
        public List<ClienteAdminItem> ObtenerClientesPaginados(string busqueda, int pagina, int cantidadPorPagina) { UltimaBusquedaClientes = busqueda; UltimaPaginaClientes = pagina; return new(); }
        public bool CambiarEstadoCliente(int id, bool activo) { UltimoClienteModificado = id; UltimoEstadoCliente = activo; return ResultadoCambioCliente; }
        public Usuario? Login(string email, string contrasenia) => null;
        public (bool Exito, string Error, int UsuarioId) RegistrarCliente(string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia) => (false, string.Empty, 0);
        public (bool Exito, string Error, int UsuarioId) RegistrarShaper(string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia, string nombreDeNegosio, string contacto) => (false, string.Empty, 0);
        public (bool Exito, string Error, int UsuarioId) RegistrarAdmin(string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia) => (false, string.Empty, 0);
        public Usuario? BuscarPorId(int id) => null;
        public Usuario? BuscarPorEmail(string email) => null;
        public List<Shaper> ObtenerShapers() => new();
        public Shaper? ObtenerShaperPorId(int id) => null;
        public int ContarClientes() => TotalClientes;
        public int ContarShapersActivos() => 0;
        public (bool Exito, string Error) ActualizarShaper(int id, string email, string nombre, Pais pais, string nombreDeNegosio, string contacto) => (false, string.Empty);
        public bool ActualizarLogoShaper(int id, string? logoUrl) => false;
        public bool CambiarEstadoShaper(int id, bool activo) => false;
        public (bool Exito, string Error) ActualizarCuenta(int id, string email, string nombre, Pais pais) => (false, string.Empty);
        public (bool Exito, string Error) CambiarContrasenia(int id, string contraseniaActual, string nuevaContrasenia, string confirmarContrasenia) => (false, string.Empty);
        public List<Shaper> ObtenerShapersPaginados(string busqueda, int pagina, int cantidadPorPagina) => new();
        public int ContarShapers(string busqueda) => 0;
    }

    private sealed class ProductoServicioEspia : IProductoServicio
    {
        public int TotalProductos { get; set; }
        public string UltimaBusqueda { get; private set; } = string.Empty;
        public string UltimoTipo { get; private set; } = string.Empty;
        public string UltimoEstado { get; private set; } = string.Empty;
        public int UltimaPagina { get; private set; }

        public int ContarProductosAdministracion(string busqueda, string tipo, string estado) { UltimaBusqueda = busqueda; UltimoTipo = tipo; UltimoEstado = estado; return TotalProductos; }
        public List<ProductoAdminItem> ObtenerProductosAdministracion(string busqueda, string tipo, string estado, int pagina, int cantidadPorPagina) { UltimaPagina = pagina; return new(); }
        public List<Producto> BuscarPorShaper(int shaperId) => new();
        public List<Tabla> ObtenerTablasDelShaper(int shaperId) => new();
        public int ContarProductosPublicados() => 0;
        public bool CambiarEstadoProducto(int id, bool oculto) => false;
        public int AgregarTabla(string titulo, string subtitulo, double precio, string descripcion, int shaperId, string altura, int ancho, double volumen, SistemaDeEncaje sistemaDeEncaje, TipoDeOla tipoDeOla, EstiloDeSurf estiloDeSurf, int pesoMinimo, int pesoMaximo, Experiencia experiencia, IFormFile imagenFrontal, IFormFile? imagenTrasera) => 0;
    }

    private sealed class PedidoServicioVacio : IPedidoServicio
    {
        public Task<List<(Pedido Pedido, string UrlPago)>> CrearPedidosDesdeCarritoAsync(int clienteId) => Task.FromResult(new List<(Pedido, string)>());
        public Task ProcesarNotificacionPagoAsync(string mercadoPagoPaymentId) => Task.CompletedTask;
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
