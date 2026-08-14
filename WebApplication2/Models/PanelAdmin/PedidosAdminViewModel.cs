using ClassLibrary.Pedidos;
using System.Collections.Generic;

namespace WebApplication2.Models.PanelAdmin
{
    public class PedidosAdminViewModel
    {
        public List<PedidoAdminItem> Pedidos { get; set; }
            = new List<PedidoAdminItem>();

        public int PaginaActual { get; set; }

        public int TotalPaginas { get; set; }

        public int TotalResultados { get; set; }
        public string Busqueda { get; set; } = string.Empty;
        public byte? EstadoId { get; set; }

        public bool TienePaginaAnterior
        {
            get
            {
                return PaginaActual > 1;
            }
        }

        public bool TienePaginaSiguiente
        {
            get
            {
                return PaginaActual < TotalPaginas;
            }
        }
    }
}
