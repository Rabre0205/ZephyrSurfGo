using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WebApplication2.Controllers;

namespace Pruebas;

public class AutorizacionControladoresTests
{
    [Theory]
    [InlineData(typeof(CarritoController), "Cliente")]
    [InlineData(typeof(DashboardController), "Shaper")]
    [InlineData(typeof(PanelAdminController), "Administrador")]
    [InlineData(typeof(ConfiguracionController), "Cliente,Shaper")]
    [InlineData(typeof(MisPedidosController), "Cliente")]
    public void CadaAreaExigeLosRolesEsperados(Type controlador, string rolesEsperados)
    {
        var autorizacion = controlador.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(autorizacion);
        Assert.Equal(rolesEsperados, autorizacion.Roles);
    }

    [Theory]
    [InlineData(typeof(CarritoController), nameof(CarritoController.Agregar))]
    [InlineData(typeof(CarritoController), nameof(CarritoController.ActualizarCantidad))]
    [InlineData(typeof(CarritoController), nameof(CarritoController.Eliminar))]
    [InlineData(typeof(CarritoController), nameof(CarritoController.Checkout))]
    [InlineData(typeof(PanelAdminController), nameof(PanelAdminController.CambiarEstadoCliente))]
    [InlineData(typeof(PanelAdminController), nameof(PanelAdminController.CambiarEstadoProducto))]
    [InlineData(typeof(PanelAdminController), nameof(PanelAdminController.CambiarEstadoShaper))]
    [InlineData(typeof(ConfiguracionController), nameof(ConfiguracionController.ActualizarCuenta))]
    [InlineData(typeof(ConfiguracionController), nameof(ConfiguracionController.CambiarContrasenia))]
    [InlineData(typeof(ConfiguracionController), nameof(ConfiguracionController.ActualizarLogo))]
    [InlineData(typeof(AdminController), nameof(AdminController.AgregarTabla))]
    [InlineData(typeof(AdminController), nameof(AdminController.RegistrarCliente))]
    public void LasOperacionesQueModificanDatosUsanPostYAntiforgery(
        Type controlador,
        string nombreAccion)
    {
        var accion = controlador.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(metodo =>
                metodo.Name == nombreAccion &&
                metodo.GetCustomAttribute<HttpPostAttribute>() != null);

        Assert.NotNull(accion.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(accion.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    [Fact]
    public void LoginNoExponeGeneradorPublicoDeHashes()
    {
        var accion = typeof(LoginController).GetMethod(
            "GenerarHash",
            BindingFlags.Instance | BindingFlags.Public);

        Assert.Null(accion);
    }
}
