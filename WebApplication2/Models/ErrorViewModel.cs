namespace WebApplication2.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public int StatusCode { get; set; } = 500;

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Titulo => StatusCode switch
        {
            403 => "Acceso denegado",
            404 => "Página no encontrada",
            _ => "Ocurrió un error"
        };

        public string Mensaje => StatusCode switch
        {
            403 => "No tenés permisos para acceder a esta sección.",
            404 => "La página o el elemento que buscás no existe.",
            _ => "No pudimos completar la operación. Intentá nuevamente más tarde."
        };
    }
}
