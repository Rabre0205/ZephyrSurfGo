using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.Cursos;

public class CursoFormModel
{
    public int Id { get; set; }
    [Required, StringLength(140,MinimumLength=4)] public string Titulo { get; set; } = string.Empty;
    [Required, StringLength(280,MinimumLength=10)] public string Resumen { get; set; } = string.Empty;
    [Required, StringLength(5000,MinimumLength=20)] public string Descripcion { get; set; } = string.Empty;
    [Required, RegularExpression("Presencial|Online|Híbrido")] public string Modalidad { get; set; } = "Presencial";
    [Required, StringLength(180)] public string Ubicacion { get; set; } = string.Empty;
    [Required] public DateTime FechaInicio { get; set; } = DateTime.Now.AddDays(30);
    [Range(0.5,500)] public decimal DuracionHoras { get; set; }
    [Range(1,500)] public int Cupos { get; set; }
    [Range(0.01,1000000)] public decimal Precio { get; set; }
    [Url, StringLength(600)] public string? ImagenUrl { get; set; }
    public bool Publicado { get; set; } = true;
}
