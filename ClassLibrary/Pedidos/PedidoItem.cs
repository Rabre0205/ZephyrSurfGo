using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Pedidos
{
    public class PedidoItem
    {
        public int ProductoId { get; set; }
        public string TituloSnapshot { get; set; } = string.Empty;
        public double PrecioUnitarioSnapshot { get; set; }
        public int Cantidad { get; set; }
    }
}
