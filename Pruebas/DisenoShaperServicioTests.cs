using ClassLibrary.Datos;
using ClassLibrary.Disenos;
using ClassLibrary.Servicios;

namespace Pruebas;

public class DisenoShaperServicioTests
{
    [Fact]
    public void GuardarAsignaElShaperAutenticadoYNormalizaColores()
    {
        var repo = new RepositorioFalso();
        var servicio = new DisenoShaperServicio(repo);
        var diseno = new DisenoShaper { Nombre="  Retro  ", ZonaAplicacion="Ambos", ColorPrimario="incorrecto", ColorSecundario="#123456" };

        var resultado = servicio.Guardar(diseno, 42);

        Assert.True(resultado.Exito);
        Assert.Equal(42, repo.Guardado!.ShaperId);
        Assert.Equal("Retro", repo.Guardado.Nombre);
        Assert.Equal("#ffffff", repo.Guardado.ColorPrimario);
        Assert.Equal("#123456", repo.Guardado.ColorSecundario);
    }

    [Theory]
    [InlineData("Lateral",0)]
    [InlineData("Deck",-1)]
    public void GuardarRechazaConfiguracionesInvalidas(string zona,decimal recargo)
    {
        var servicio=new DisenoShaperServicio(new RepositorioFalso());
        var resultado=servicio.Guardar(new DisenoShaper{Nombre="Diseño",ZonaAplicacion=zona,Recargo=recargo},5);
        Assert.False(resultado.Exito);
    }

    private sealed class RepositorioFalso : IDisenoShaperRepositorio
    {
        public DisenoShaper? Guardado { get; private set; }
        public List<DisenoShaper> ObtenerPorShaper(int shaperId,bool soloActivos=false)=>new();
        public DisenoShaper? ObtenerPorId(int id)=>null;
        public int Insertar(DisenoShaper diseno){Guardado=diseno;return 1;}
        public bool Actualizar(DisenoShaper diseno){Guardado=diseno;return true;}
        public bool CambiarEstado(int id,int shaperId,bool activo)=>true;
    }
}
