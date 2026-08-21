using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Carrito
{
    public class CarritoItemDetallado
    {
        public int ProductoId { get; set; }
        public string TipoProducto { get; set; }
        public string Titulo { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
        public double Precio { get; set; }
        public int ShaperId { get; set; }
        public int Cantidad { get; set; }
        public int? StockDisponible { get; set; }
        public bool Disponible { get; set; }
    }
}
