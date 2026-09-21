using ClassLibrary.Enums;

namespace ClassLibrary.PuntosRetiro;

public class PuntoRetiro
{
    public int Id { get; set; }
    public int ShaperId { get; set; }
    public string ShaperNombre { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
    public string Indicaciones { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public bool Activo { get; set; } = true;
    public Pais Pais { get; set; }
    public DateTime FechaCreacion { get; set; }
}
