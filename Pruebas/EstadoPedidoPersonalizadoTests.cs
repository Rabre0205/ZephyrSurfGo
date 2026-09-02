using ClassLibrary.Solicitudes;

namespace Pruebas;

public class EstadoPedidoPersonalizadoTests
{
    [Theory]
    [InlineData(0, "Esperando cotización", 1)]
    [InlineData(1, "Cotizado", 2)]
    [InlineData(3, "Aceptado por el cliente", 3)]
    [InlineData(5, "Esperando pago", 4)]
    [InlineData(6, "Pagado", 5)]
    [InlineData(7, "En preparación", 6)]
    [InlineData(8, "Enviado o listo para retirar", 7)]
    [InlineData(9, "Entregado", 8)]
    public void ExponeElNombreYPasoCorrectos(byte estado, string nombre, int paso)
    {
        var pedido = new SolicitudPersonalizada { Estado = estado };

        Assert.Equal(nombre, pedido.EstadoNombre);
        Assert.Equal(paso, pedido.PasoVisible);
    }
}
