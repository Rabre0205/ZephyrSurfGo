namespace ClassLibrary.Soporte;

public class SolicitudSoporte
{
    public int Id { get; set; }
    public int ShaperId { get; set; }
    public string ShaperNombre { get; set; } = string.Empty;
    public string ShaperEmail { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Respuesta { get; set; }
    public byte Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public string EstadoNombre => Estado switch { 1 => "Respondida", 2 => "Cerrada", _ => "Abierta" };
}
