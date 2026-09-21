namespace ClassLibrary.Interacciones;

public class ResenaProducto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";
    public byte Estrellas { get; set; }
    public string Comentario { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string RespuestaShaper { get; set; } = "";
    public DateTime? FechaRespuesta { get; set; }
    public bool Moderada { get; set; }
    public string MotivoModeracion { get; set; } = "";
    public string ProductoTitulo { get; set; } = "";
    public string ShaperNombre { get; set; } = "";
}

public record ResumenResenas(double Promedio, int Cantidad);
