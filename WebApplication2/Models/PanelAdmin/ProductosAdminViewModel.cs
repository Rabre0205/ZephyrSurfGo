using ClassLibrary.Productos;

namespace WebApplication2.Models.PanelAdmin
{
    public class ProductosAdminViewModel
    {
        public List<ProductoAdminItem> Productos { get; set; } = new();
        public string Busqueda { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalResultados { get; set; }
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
        public IReadOnlyList<string> Tipos { get; } =
            new[] { "Tabla", "Leash", "Pad", "Quilla", "Traje" };
    }
}
