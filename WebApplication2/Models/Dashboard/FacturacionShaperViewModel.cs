using ClassLibrary.Pedidos;

namespace WebApplication2.Models.Dashboard
{
    public class FacturacionShaperViewModel
    {
        public decimal VentasConfirmadas { get; set; }
        public decimal Comisiones { get; set; }
        public decimal IngresoNeto => VentasConfirmadas - Comisiones;
        public List<PedidoAdminItem> Movimientos { get; set; } = new();
    }
}
