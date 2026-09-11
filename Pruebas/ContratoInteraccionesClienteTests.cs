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
        AssertPostSeguro(typeof(DisenosGuardadosController), "Guardar", 3);
        AssertPostSeguro(typeof(DisenosGuardadosController), "Eliminar", 1);
    }

    [Fact]
    public void MigracionRestringeEstrellasDuplicadosYReferencias()
    {
        string sql = File.ReadAllText(Path.Combine(BuscarRaizProyecto(), "Database", "AgregarResenasFavoritosYDisenosGuardados.sql"));
        Assert.Contains("CHECK (Estrellas BETWEEN 1 AND 5)", sql);
        Assert.Contains("UNIQUE (ClienteId, ProductoId)", sql);
        Assert.Contains("REFERENCES dbo.Pedidos(Id)", sql);
        Assert.Contains("CHECK (ISJSON(ConfiguracionJson)=1)", sql);
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
