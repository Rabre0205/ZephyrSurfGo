using ClassLibrary.Pedidos;
using ClassLibrary.Solicitudes;

namespace WebApplication2.Models.MisPedidos;

public class MisPedidosViewModel
{
    public List<PedidoAdminItem> Compras { get; set; } = new();
    public List<SolicitudPersonalizada> Personalizados { get; set; } = new();
    public bool EstaVacio => Compras.Count == 0 && Personalizados.Count == 0;
}
