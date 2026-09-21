namespace ClassLibrary.Interacciones;

public class FavoritoProducto
{
    public int ProductoId { get; set; }
    public int ShaperId { get; set; }
    public string Titulo { get; set; } = "";
    public string Subtitulo { get; set; } = "";
    public string ImagenUrl { get; set; } = "";
    public decimal Precio { get; set; }
    public decimal PrecioGuardado { get; set; }
    public bool Disponible { get; set; }
    public int? Stock { get; set; }
    public bool CambioPrecio => Precio != PrecioGuardado;
    public string TipoProducto { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
}
