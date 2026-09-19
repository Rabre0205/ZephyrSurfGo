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
    public bool Publicado { get; set; }
    public string ShaperNombre { get; set; } = string.Empty;
    public string NegocioShaper { get; set; } = string.Empty;
    public int CuposDisponibles => Math.Max(0, Cupos - CuposOcupados);
}

public class InscripcionCurso
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal PorcentajeComision { get; set; }
    public decimal ComisionPlataforma { get; set; }
    public decimal NetoShaper { get; set; }
    public byte Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
