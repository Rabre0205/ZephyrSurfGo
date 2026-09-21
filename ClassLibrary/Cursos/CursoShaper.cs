namespace ClassLibrary.Cursos;

public class CursoShaper
{
    public int Id { get; set; }
    public int ShaperId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Modalidad { get; set; } = "Presencial";
    public string Ubicacion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public decimal DuracionHoras { get; set; }
    public int Cupos { get; set; }
    public int CuposOcupados { get; set; }
    public decimal Precio { get; set; }
    public string? ImagenUrl { get; set; }
    public string InstructorNombre { get; set; } = string.Empty;
    public string InstructorBio { get; set; } = string.Empty;
    public string PublicoObjetivo { get; set; } = string.Empty;
    public string Incluye { get; set; } = string.Empty;
    public string Requisitos { get; set; } = string.Empty;
    public string? CronogramaUrl { get; set; }
    public string? Contacto { get; set; }
    public byte CuotasMaximas { get; set; } = 1;
    public decimal DescuentoAcompanantePorcentaje { get; set; }
    public bool HospedajeDisponible { get; set; }
    public bool Publicado { get; set; }
    public string ShaperNombre { get; set; } = string.Empty;
    public string NegocioShaper { get; set; } = string.Empty;
    public List<CursoModulo> Modulos { get; set; } = new();
    public List<string> Imagenes { get; set; } = new();
    public int CuposDisponibles => Math.Max(0, Cupos - CuposOcupados);
}

public class CursoModulo
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Encuentros { get; set; }
    public decimal DuracionHoras { get; set; }
    public decimal? Precio { get; set; }
    public bool PermiteCompraIndividual { get; set; }
    public int Orden { get; set; }
}

public class InscripcionCurso
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public int ClienteId { get; set; }
    public int? ModuloId { get; set; }
    public string OpcionElegida { get; set; } = "Workshop completo";
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal PorcentajeComision { get; set; }
    public decimal ComisionPlataforma { get; set; }
    public decimal NetoShaper { get; set; }
    public byte Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
