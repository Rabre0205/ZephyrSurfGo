using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Controllers;

namespace Pruebas;

public class ContratoInteraccionesClienteTests
{
    [Theory]
    [InlineData(typeof(FavoritosController))]
    [InlineData(typeof(ResenasController))]
    [InlineData(typeof(DisenosGuardadosController))]
    public void FuncionesPrivadasRequierenRolCliente(Type controlador)
    {
        var atributo = Assert.Single(controlador.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Cliente", atributo.Roles);
    }

    [Fact]
    public void PublicarResenaYModificarListasExigenAntifalsificacion()
    {
        AssertPostSeguro(typeof(ResenasController), "Crear", 4);
        AssertPostSeguro(typeof(FavoritosController), "Alternar", 2);
        AssertPostSeguro(typeof(FavoritosController), "EliminarSeleccionados", 1);
        AssertPostSeguro(typeof(DisenosGuardadosController), "Guardar", 3);
        AssertPostSeguro(typeof(DisenosGuardadosController), "Eliminar", 1);
        AssertPostSeguro(typeof(DisenosGuardadosController), "EliminarSeleccionados", 1);
        AssertPostSeguro(typeof(DisenosGuardadosController), "Renombrar", 2);
        AssertPostSeguro(typeof(DisenosGuardadosController), "Duplicar", 2);
    }

    [Fact]
    public void MigracionRestringeEstrellasDuplicadosYReferencias()
    {
        string sql = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "Database", "AgregarResenasFavoritosYDisenosGuardados.sql"));
        Assert.Contains("CHECK (Estrellas BETWEEN 1 AND 5)", sql);
        Assert.Contains("UNIQUE (ClienteId, ProductoId)", sql);
        Assert.Contains("REFERENCES dbo.Pedidos(Id)", sql);
        Assert.Contains("CHECK (ISJSON(ConfiguracionJson)=1)", sql);
        Assert.Contains("PrecioGuardado", sql);
        Assert.Contains("FechaActualizacion", sql);
    }

    [Theory]
    [InlineData("ResenasProductos")]
    [InlineData("FavoritosProductos")]
    [InlineData("DisenosGuardados")]
    [InlineData("PuntosRetiro")]
    [InlineData("SolicitudesSoporte")]
    [InlineData("SolicitudesPersonalizadas")]
    [InlineData("DisenosShaper")]
    [InlineData("EntregaEstimada")]
    [InlineData("EntregaOriginal")]
    [InlineData("AvanceShaper")]
    [InlineData("FechaSeguimiento")]
    public void ArchivoPrincipalContieneTodaLaEstructuraAgregada(string elemento)
    {
        string sql = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "Database", "CreacionSurfDB.sql"));
        Assert.Contains(elemento, sql);
    }

    [Fact]
    public void SolicitudPublicaDeShaperProtegeElEnvio()
    {
        AssertPostSeguro(typeof(SurfController), "SolicitarIngresoShaper", 1);
        var accionLista = Assert.Single(typeof(SurfController).GetMethods(), m => m.Name == "shapers" && m.GetParameters().Length == 0);
        var accionSolicitud = Assert.Single(typeof(SurfController).GetMethods(), m => m.Name == "SolicitarIngresoShaper");
        Assert.Empty(accionLista.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Empty(accionSolicitud.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        string vista = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "WebApplication2", "Views", "Surf", "shapers.cshtml"));
        Assert.Contains("asp-for=\"Solicitud.AceptaContacto\"", vista);
        Assert.Contains("@Html.AntiForgeryToken()", vista);
    }

    [Fact]
    public void DetalleDeProductoExponeDisponibilidadEdicionYRelacionados()
    {
        string vista = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "WebApplication2", "Views", "Shaper", "Producto.cshtml"));
        Assert.Contains("Model.Disponible", vista);
        Assert.Contains("disabled=\"@(!Model.Disponible)\"", vista);
        Assert.Contains("asp-action=\"Editar\"", vista);
        Assert.Contains("Model.Relacionados", vista);
        Assert.Contains("Model.Stock", vista);
        Assert.Contains("aria-pressed", vista);
    }

    [Fact]
    public void MapaDeRetirosIncluyeFiltrosUbicacionYEstadosAccesibles()
    {
        string vista = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "WebApplication2", "Views", "Surf", "Dealers.cshtml"));
        string script = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "WebApplication2", "wwwroot", "js", "Dealers.js"));
        Assert.Contains("nearMeButton", vista);
        Assert.Contains("clearFiltersButton", vista);
        Assert.Contains("aria-live=\"polite\"", vista);
        Assert.Contains("pickup.notes", script);
        Assert.Contains("fitBounds", script);
        Assert.Contains("navigator.geolocation", script);
        Assert.Contains("tileerror", script);
        string estilosMapa = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "WebApplication2", "wwwroot", "css", "estilosDealers.css"));
        Assert.Contains(".editorial-map .search-wrap input", estilosMapa);
        Assert.Contains("text-overflow:ellipsis", estilosMapa);
    }

    [Fact]
    public void CatalogoDeShapersUsaTarjetasCompactasYResponsive()
    {
        string estilos = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "WebApplication2", "wwwroot", "css", "estilosShapers.css"));
        Assert.Contains("grid-template-columns:minmax(180px,38%) 1fr", estilos);
        Assert.Contains("-webkit-line-clamp:3", estilos);
        Assert.Contains(".editorial-shapers .shaper-card{grid-template-columns:1fr", estilos);
    }

    [Fact]
    public void FavoritosYDisenosOfrecenGestionCompleta()
    {
        string raiz=BuscarRaizProyecto();
        string favoritos=File.ReadAllText(Path.Combine(raiz,"WebApplication2","Views","Favoritos","Index.cshtml"));
        string disenos=File.ReadAllText(Path.Combine(raiz,"WebApplication2","Views","DisenosGuardados","Index.cshtml"));
        Assert.Contains("CambioPrecio",favoritos);
        Assert.Contains("No disponible",favoritos);
        Assert.Contains("EliminarSeleccionados",favoritos);
        Assert.Contains("Continuar y comprar",disenos);
        Assert.Contains("data-open-rename",disenos);
        Assert.Contains("data-open-duplicate",disenos);
    }

    private static void AssertPostSeguro(Type controlador, string accion, int parametros)
    {
        var metodo = Assert.Single(controlador.GetMethods(), m => m.Name == accion && m.GetParameters().Length == parametros);
        Assert.NotNull(metodo.GetCustomAttributes(typeof(HttpPostAttribute), true).SingleOrDefault());
        Assert.NotNull(metodo.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true).SingleOrDefault());
    }

    private static string BuscarRaizProyecto()
    {
        DirectoryInfo? directorio = new(AppContext.BaseDirectory);
        while (directorio != null)
        {
            if (Directory.Exists(Path.Combine(directorio.FullName, "WebApplication2"))) return directorio.FullName;
            directorio = directorio.Parent;
        }
        throw new DirectoryNotFoundException("No se encontró la raíz de Zephyr Surf Go.");
    }
}
