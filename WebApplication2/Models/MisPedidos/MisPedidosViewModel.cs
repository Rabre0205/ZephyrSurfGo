using ClassLibrary.Pedidos;
using ClassLibrary.Solicitudes;

namespace WebApplication2.Models.MisPedidos;

public class MisPedidosViewModel
{
    public List<PedidoAdminItem> Compras { get; set; } = new();
    public List<SolicitudPersonalizada> Personalizados { get; set; } = new();
    public bool EstaVacio => Compras.Count == 0 && Personalizados.Count == 0;
    public int CantidadEnCurso => Compras.Count(x => x.EstadoId != 4) +
        Personalizados.Count(x => x.Estado is not (2 or 4 or 9));
    public int CantidadHistorial => Compras.Count(x => x.EstadoId == 4) +
        Personalizados.Count(x => x.Estado is 2 or 4 or 9);
}
