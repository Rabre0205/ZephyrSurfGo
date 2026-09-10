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
}

public record ResumenResenas(double Promedio, int Cantidad);
