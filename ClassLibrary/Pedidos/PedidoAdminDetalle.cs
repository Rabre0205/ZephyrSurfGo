namespace ClassLibrary.Pedidos
{
    public class PedidoAdminDetalle : PedidoAdminItem
    {
        public string MercadoPagoPreferenceId { get; set; } = string.Empty;
        public string MercadoPagoPaymentId { get; set; } = string.Empty;
        public string PuntoRetiroNombre { get; set; } = string.Empty;
        public string PuntoRetiroDireccion { get; set; } = string.Empty;
        public string PuntoRetiroCiudad { get; set; } = string.Empty;
        public string PuntoRetiroHorario { get; set; } = string.Empty;
        public string PuntoRetiroIndicaciones { get; set; } = string.Empty;
        public List<PedidoItem> Items { get; set; } = new();
    }
}
