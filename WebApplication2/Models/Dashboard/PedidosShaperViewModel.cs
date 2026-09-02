using ClassLibrary.Pedidos;
using ClassLibrary.Solicitudes;

namespace WebApplication2.Models.Dashboard
{
    public class PedidosShaperViewModel
    {
        public List<PedidoAdminItem> Pedidos { get; set; } = new();
        public List<SolicitudPersonalizada> Personalizados { get; set; } = new();
        public string Busqueda { get; set; } = string.Empty;
        public byte? EstadoId { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalResultados { get; set; }
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
