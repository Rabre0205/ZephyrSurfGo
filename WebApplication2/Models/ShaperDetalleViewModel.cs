using ClassLibrary.Persona;
using ClassLibrary.Productos;
using ClassLibrary.Disenos;
using ClassLibrary.Interacciones;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication2.Models
{
    public class ShaperDetalleViewModel
    {
        public Shaper Shaper { get; set; } = null!;

        public List<Producto> Productos { get; set; } = new List<Producto>();
        public List<DisenoShaper> Disenos { get; set; } = new();
        public Dictionary<int, ResumenResenas> Resenas { get; set; } = new();
        public HashSet<int> Favoritos { get; set; } = new();
        public string? ConfiguracionGuardadaJson { get; set; }

        public List<Tabla> Tablas => Productos.OfType<Tabla>().ToList();
        public List<Quilla> Quillas => Productos.OfType<Quilla>().ToList();
        public List<Leash> Leashes => Productos.OfType<Leash>().ToList();
        public List<Pad> Pads => Productos.OfType<Pad>().ToList();
        public List<Producto> Accesorios => Productos.Where(producto => producto is not Tabla).ToList();
    }
}
