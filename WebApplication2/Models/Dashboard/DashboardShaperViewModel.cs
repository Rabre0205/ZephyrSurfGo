using ClassLibrary.Pedidos;

namespace WebApplication2.Models.Dashboard
{
    public class DashboardShaperViewModel
    {
        public int TotalPedidos { get; set; }
        public int PedidosPendientes { get; set; }
        public int ProductosPublicados { get; set; }
        public decimal VentasConfirmadas { get; set; }
        public decimal Comisiones { get; set; }
        public decimal IngresoNeto => VentasConfirmadas - Comisiones;
        public List<PedidoAdminItem> PedidosRecientes { get; set; } = new();
    }
}
