namespace ClassLibrary.Pedidos
{
    public class PedidoAdminDetalle : PedidoAdminItem
    {
        public string MercadoPagoPreferenceId { get; set; } = string.Empty;
        public string MercadoPagoPaymentId { get; set; } = string.Empty;
        public List<PedidoItem> Items { get; set; } = new();
    }
}
