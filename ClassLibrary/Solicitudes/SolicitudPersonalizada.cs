namespace ClassLibrary.Solicitudes;

public class SolicitudPersonalizada
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ShaperId { get; set; }
    public int? ProductoBaseId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ShaperNombre { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public decimal PrecioEstimado { get; set; }
    public string Largo { get; set; } = string.Empty;
    public string Ancho { get; set; } = string.Empty;
    public string Grosor { get; set; } = string.Empty;
    public string Volumen { get; set; } = string.Empty;
    public string Construccion { get; set; } = string.Empty;
    public string Tail { get; set; } = string.Empty;
    public string SistemaQuillas { get; set; } = string.Empty;
    public string ConfiguracionQuillas { get; set; } = string.Empty;
    public string Laminado { get; set; } = string.Empty;
    public string ParcheCarbono { get; set; } = string.Empty;
    public string Diseno { get; set; } = string.Empty;
    public string ColorPrimario { get; set; } = string.Empty;
    public string ColorSecundario { get; set; } = string.Empty;
    public string DetallesAdicionales { get; set; } = string.Empty;
    public string AccesoriosJson { get; set; } = "[]";
    public string Notas { get; set; } = string.Empty;
    public byte Estado { get; set; }
    public DateTime FechaCreacion { get; set; }

    public string EstadoNombre => Estado switch
    {
        1 => "Precio definido",
        2 => "No disponible",
        _ => "Pendiente de revisión"
    };
}
