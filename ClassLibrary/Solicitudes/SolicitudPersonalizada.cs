using ClassLibrary.Enums;

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
    public DateTime? FechaRespuestaCliente { get; set; }
    public EstadoPedidoPersonalizado EstadoFlujo => (EstadoPedidoPersonalizado)Estado;

    public string EstadoNombre => Estado switch
    {
        1 => "Cotizado",
        2 => "No disponible",
        3 => "Aceptado por el cliente",
        4 => "Rechazado por el cliente",
        5 => "Esperando pago",
        6 => "Pagado",
        7 => "En preparación",
        8 => "Enviado o listo para retirar",
        9 => "Entregado",
        _ => "Esperando cotización"
    };

    public int PasoVisible => Estado switch
    {
        0 or 2 or 4 => 1,
        1 => 2,
        3 => 3,
        5 => 4,
        6 => 5,
        7 => 6,
        8 => 7,
        9 => 8,
        _ => 1
    };
}
