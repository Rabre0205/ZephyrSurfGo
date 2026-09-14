using ClassLibrary.Datos;
using ClassLibrary.PuntosRetiro;
using ClassLibrary.Servicios;
using Xunit;

namespace Pruebas;

public class PuntoRetiroServicioTests
{
    [Fact]
    public void Guardar_RechazaCoordenadasSinMarcar()
    {
        var repo = new RepositorioFalso();
        var resultado = new PuntoRetiroServicio(repo).Guardar(new PuntoRetiro
        {
            Nombre="Local", Direccion="Calle 1", Ciudad="Montevideo"
        }, 7);

        Assert.False(resultado.Exito);
        Assert.Contains("ubicación exacta", resultado.Error);
        Assert.Empty(repo.Puntos);
    }

    [Fact]
    public void Eliminar_NoPermiteEliminarPuntoDeOtroShaper()
    {
        var repo = new RepositorioFalso();
        repo.Puntos.Add(new PuntoRetiro { Id=3, ShaperId=8, Nombre="Otro" });

        Assert.False(new PuntoRetiroServicio(repo).Eliminar(3, 7));
        Assert.Single(repo.Puntos);
    }

    [Fact]
    public void Eliminar_EliminaPuntoPropio()
    {
        var repo = new RepositorioFalso();
        repo.Puntos.Add(new PuntoRetiro { Id=3, ShaperId=7, Nombre="Propio" });

        Assert.True(new PuntoRetiroServicio(repo).Eliminar(3, 7));
        Assert.Empty(repo.Puntos);
    }

    private sealed class RepositorioFalso : IPuntoRetiroRepositorio
    {
        public List<PuntoRetiro> Puntos { get; } = new();
        public List<PuntoRetiro> ObtenerPorShaper(int shaperId)=>Puntos.Where(p=>p.ShaperId==shaperId).ToList();
        public List<PuntoRetiro> ObtenerActivos()=>Puntos.Where(p=>p.Activo).ToList();
        public PuntoRetiro? ObtenerPorId(int id)=>Puntos.SingleOrDefault(p=>p.Id==id);
        public int Insertar(PuntoRetiro punto){punto.Id=Puntos.Count+1;Puntos.Add(punto);return punto.Id;}
        public bool Actualizar(PuntoRetiro punto)=>ObtenerPorId(punto.Id)!=null;
        public bool CambiarEstado(int id,int shaperId,bool activo){var p=Puntos.SingleOrDefault(x=>x.Id==id&&x.ShaperId==shaperId);if(p==null)return false;p.Activo=activo;return true;}
        public bool Eliminar(int id,int shaperId){var p=Puntos.SingleOrDefault(x=>x.Id==id&&x.ShaperId==shaperId);return p!=null&&Puntos.Remove(p);}
    }
}
