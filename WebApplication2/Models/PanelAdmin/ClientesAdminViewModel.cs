using ClassLibrary.Persona;

namespace WebApplication2.Models.PanelAdmin
{
    public class ClientesAdminViewModel
    {
        public List<ClienteAdminItem> Clientes { get; set; } = new();
        public string Busqueda { get; set; } = string.Empty;
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalResultados { get; set; }
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
