using System;

namespace ClassLibrary.Pedidos
{
    public class PedidoAdminItem
    {
        public int Id { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public string ClienteEmail { get; set; } = string.Empty;

        public string ShaperNombre { get; set; } = string.Empty;

        public string NegocioShaper { get; set; } = string.Empty;

        public byte EstadoId { get; set; }

        public string EstadoNombre { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public decimal ComisionPlataforma { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}