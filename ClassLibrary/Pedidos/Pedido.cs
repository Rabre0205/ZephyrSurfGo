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
        public string MercadoPagoPreferenceId { get; set; } = string.Empty;
        public string MercadoPagoPaymentId { get; set; } = string.Empty;
        public int? PuntoRetiroId { get; set; }
        public string PuntoRetiroNombre { get; set; } = string.Empty;
        public string PuntoRetiroDireccion { get; set; } = string.Empty;
        public string PuntoRetiroCiudad { get; set; } = string.Empty;
        public string PuntoRetiroHorario { get; set; } = string.Empty;
        public string PuntoRetiroIndicaciones { get; set; } = string.Empty;
        public decimal? PuntoRetiroLatitud { get; set; }
        public decimal? PuntoRetiroLongitud { get; set; }
        public List<PedidoItem> Items { get; set; } = new List<PedidoItem>();
    }
}
