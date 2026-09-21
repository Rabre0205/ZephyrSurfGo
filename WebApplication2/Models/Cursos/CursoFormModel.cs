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
    [Required, StringLength(140)] public string InstructorNombre { get; set; } = string.Empty;
    [Required, StringLength(1800,MinimumLength=20)] public string InstructorBio { get; set; } = string.Empty;
    [Required, StringLength(2500,MinimumLength=10)] public string PublicoObjetivo { get; set; } = string.Empty;
    [Required, StringLength(3000,MinimumLength=10)] public string Incluye { get; set; } = string.Empty;
    [StringLength(1800)] public string Requisitos { get; set; } = string.Empty;
    [Url, StringLength(600)] public string? CronogramaUrl { get; set; }
    [StringLength(500)] public string? Contacto { get; set; }
    [Range(1,24)] public byte CuotasMaximas { get; set; } = 1;
    [Range(0,100)] public decimal DescuentoAcompanantePorcentaje { get; set; }
    public bool HospedajeDisponible { get; set; }
    [StringLength(4000)] public string? GaleriaUrls { get; set; }
    public List<CursoModuloFormModel> Modulos { get; set; } = new();
    public bool Publicado { get; set; } = true;
}

public class CursoModuloFormModel
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string Descripcion { get; set; } = string.Empty;
    [Range(1,100)] public int Encuentros { get; set; } = 1;
    [Range(0.5,500)] public decimal DuracionHoras { get; set; }
    [Range(0.01,1000000)] public decimal? Precio { get; set; }
    public bool PermiteCompraIndividual { get; set; }
}
