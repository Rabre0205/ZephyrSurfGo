namespace ClassLibrary.Enums;

public enum EstadoPedidoPersonalizado : byte
{
    EsperandoCotizacion = 0,
    Cotizado = 1,
    NoDisponible = 2,
    Aceptado = 3,
    Rechazado = 4,
    EsperandoPago = 5,
    Pagado = 6,
    EnPreparacion = 7,
    EnviadoOListoParaRetirar = 8,
    Entregado = 9
}
