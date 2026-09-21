namespace WebApplication2.Models.PanelAdmin
{
    public class EstadisticasAdminViewModel
    {
        public int TotalPedidos { get; set; }

        public int PedidosPendientes { get; set; }

        public int PedidosAprobados { get; set; }

        public int PedidosRechazados { get; set; }

        public int PedidosCancelados { get; set; }

        public int PedidosCompletados { get; set; }

        public decimal VentasTotales { get; set; }

        public decimal ComisionTotal { get; set; }

        public decimal TicketPromedio { get; set; }

        public int TotalShapers { get; set; }

        public int ShapersActivos { get; set; }

        public int TotalClientes { get; set; }

        public int TotalProductos { get; set; }
    }
}