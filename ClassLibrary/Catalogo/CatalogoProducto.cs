namespace ClassLibrary.Catalogo;

public class CatalogoProducto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Subtitulo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public decimal Precio { get; set; }
    public string ImagenUrl { get; set; } = "";
    public int ShaperId { get; set; }
    public string Shaper { get; set; } = "";
    public string Pais { get; set; } = "";
    public bool Disponible { get; set; }
    public double PromedioResenas { get; set; }
    public int CantidadResenas { get; set; }
}
