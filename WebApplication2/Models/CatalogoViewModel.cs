using ClassLibrary.Catalogo;
namespace WebApplication2.Models;
public class CatalogoViewModel
{
    public List<CatalogoProducto> Productos { get; set; }=[];
    public string Busqueda { get; set; }=""; public string Tipo { get; set; }=""; public decimal? PrecioMin { get; set; } public decimal? PrecioMax { get; set; }
    public bool SoloDisponibles { get; set; } public string Ordenar { get; set; }="nuevos"; public int Pagina { get; set; }=1; public int TotalPaginas { get; set; } public int Total { get; set; }
}
