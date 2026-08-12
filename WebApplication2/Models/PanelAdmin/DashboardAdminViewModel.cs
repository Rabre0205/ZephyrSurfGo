namespace WebApplication2.Models.PanelAdmin
{
    public class DashboardAdminViewModel
    {
        public int TotalShapers { get; set; }

        public int ShapersActivos { get; set; }

        public int TotalClientes { get; set; }

        public int TotalProductos { get; set; }

        public int TotalPedidos { get; set; }

        public decimal VentasTotales { get; set; }

        public decimal ComisionTotal { get; set; }
    }
}