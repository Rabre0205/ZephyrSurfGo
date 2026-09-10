using ClassLibrary.Solicitudes;
namespace Pruebas;
public class SeguimientoPersonalizadoTests
{
    [Theory]
    [InlineData(0,0,true)]
    [InlineData(3,3,true)]
    [InlineData(6,7,true)]
    [InlineData(7,8,true)]
    [InlineData(8,9,true)]
    [InlineData(3,6,false)]
    [InlineData(5,6,false)]
    [InlineData(1,7,false)]
    [InlineData(6,9,false)]
    [InlineData(8,7,false)]
    [InlineData(9,9,false)]
    [InlineData(2,2,false)]
    public void SoloPermiteEtapasReales(byte actual,byte siguiente,bool esperado)
        =>Assert.Equal(esperado,ReglasSeguimiento.Permite(actual,siguiente));
}
