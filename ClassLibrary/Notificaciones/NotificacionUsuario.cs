namespace ClassLibrary.Notificaciones;

public class NotificacionUsuario
{
    public long Id { get; set; }
    public int UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string Tipo { get; set; } = "General";
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
}
