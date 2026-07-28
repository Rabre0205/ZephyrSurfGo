using ClassLibrary.Persona;
using ClassLibrary.Productos;
using System.Collections.Generic;

namespace WebApplication2.Models
{
    public class ShaperDetalleViewModel
    {
        public Shaper Shaper { get; set; } = null!;

        public List<Producto> Productos { get; set; } = new List<Producto>();
    }
}
