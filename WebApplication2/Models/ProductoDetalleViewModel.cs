using ClassLibrary.Interacciones;
using ClassLibrary.Persona;
using ClassLibrary.Productos;

namespace WebApplication2.Models;

public class ProductoDetalleViewModel
{
    public Producto Producto { get; set; } = null!;
    public Shaper Shaper { get; set; } = null!;
    public List<ResenaProducto> Resenas { get; set; } = new();
    public bool EsFavorito { get; set; }
    public string Tipo => Producto.GetType().Name;
    public double Promedio => Resenas.Count == 0 ? 0 : Resenas.Average(r => r.Estrellas);
}
