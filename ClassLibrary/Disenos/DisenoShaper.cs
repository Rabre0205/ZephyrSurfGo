namespace ClassLibrary.Disenos;

public class DisenoShaper
{
    public int Id { get; set; }
    public int ShaperId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public string ZonaAplicacion { get; set; } = "Ambos";
    public bool PermiteColoresPersonalizados { get; set; } = true;
    public string ColorPrimario { get; set; } = "#ffffff";
    public string ColorSecundario { get; set; } = "#111111";
    public decimal Recargo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
}
