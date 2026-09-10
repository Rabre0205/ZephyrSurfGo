namespace Pruebas;

public class ContratoCustomizadorTests
{
    private static readonly string Raiz = BuscarRaizProyecto();

    [Fact]
    public void CatalogoCambiaAlDorsoConHoverYSinBotones()
    {
        string vista = Leer("WebApplication2", "Views", "Shaper", "Detalle.cshtml");

        Assert.Contains("onmouseenter=\"showCatalogBoardSideFromGallery(this, 'back')\"", vista);
        Assert.Contains("onmouseleave=\"showCatalogBoardSideFromGallery(this, 'front')\"", vista);
        Assert.DoesNotContain("showCatalogBoardSide(this, 'front')", vista);
        Assert.DoesNotContain("showCatalogBoardSide(this, 'back')", vista);
    }

    [Fact]
    public void CustomizadorNoRenderizaCapasDeAccesorios()
    {
        foreach (string archivo in new[]
        {
            Leer("WebApplication2", "Views", "Shaper", "Detalle.cshtml"),
            Leer("WebApplication2", "Views", "Surf", "master.cshtml")
        })
        {
            Assert.DoesNotContain("id=\"deckGrip\"", archivo);
            Assert.DoesNotContain("id=\"bottomFins\"", archivo);
            Assert.DoesNotContain("id=\"deckCarbon\"", archivo);
            Assert.DoesNotContain("id=\"bottomCarbon\"", archivo);
        }
    }

    [Fact]
    public void PedidoPersonalizadoSiempreSeEnviaSinAccesorios()
    {
        string javascript = Leer("WebApplication2", "wwwroot", "js", "Master.js");

        Assert.Contains("AccesoriosJson: '[]'", javascript);
        Assert.DoesNotContain("AccesoriosJson: JSON.stringify", javascript);
    }

    [Fact]
    public void FrenteYDorsoUsanLaMismaSiluetaLimpia()
    {
        string javascript = Leer("WebApplication2", "wwwroot", "js", "Master.js");
        string estilos = Leer("WebApplication2", "wwwroot", "css", "estilosMaster.css");

        Assert.Contains("if (deckBase) deckBase.src = '/img/boards/deck-mask.png';", javascript);
        Assert.Contains("if (bottomBase) bottomBase.src = '/img/boards/deck-mask.png';", javascript);
        Assert.Contains("#bottomBase", estilos);
        Assert.Contains(".bottom-paint-layer", estilos);
    }

    [Fact]
    public void FormularioYServidorExigenAmbasImagenes()
    {
        string vista = Leer("WebApplication2", "Views", "Productos", "Crear.cshtml");
        string controlador = Leer("WebApplication2", "Controllers", "ProductosController.cs");

        Assert.Contains("asp-for=\"ImagenFrontal\"", vista);
        Assert.Contains("asp-for=\"ImagenTrasera\" type=\"file\"", vista);
        Assert.Contains("accept=\".jpg,.jpeg,.png,.webp\" required", vista);
        Assert.Contains("La imagen frontal es obligatoria.", controlador);
        Assert.Contains("La imagen trasera es obligatoria.", controlador);
    }

    private static string Leer(params string[] partes) =>
        File.ReadAllText(Path.Combine([Raiz, .. partes]));

    private static string BuscarRaizProyecto()
    {
        DirectoryInfo? directorio = new(AppContext.BaseDirectory);
        while (directorio != null)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "ZephyrSurfGo.sln")) ||
                Directory.Exists(Path.Combine(directorio.FullName, "WebApplication2")))
            {
                return directorio.FullName;
            }

            directorio = directorio.Parent;
        }

        throw new DirectoryNotFoundException("No se encontró la raíz de Zephyr Surf Go.");
    }
}
