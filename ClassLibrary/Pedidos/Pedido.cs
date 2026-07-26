using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Pedidos
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ShaperId { get; set; }
        public byte EstadoPedidoId { get; set; }
        public double Total { get; set; }
        public double ComisionPlataforma { get; set; }
        public string MercadoPagoPreferenceId { get; set; }
        public string MercadoPagoPaymentId { get; set; }
        public List<PedidoItem> Items { get; set; } = new List<PedidoItem>();
    }
}
