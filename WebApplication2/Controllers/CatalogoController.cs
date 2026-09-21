using ClassLibrary.Datos; using Microsoft.AspNetCore.Mvc; using WebApplication2.Models;
namespace WebApplication2.Controllers;
public class CatalogoController(ICatalogoRepositorio repositorio):Controller
{
    public IActionResult Index(string busqueda="",string tipo="",decimal? precioMin=null,decimal? precioMax=null,bool soloDisponibles=false,string ordenar="nuevos",int pagina=1)
    {
        busqueda=(busqueda??"").Trim(); tipo=(tipo??"").Trim(); pagina=Math.Max(1,pagina); const int cantidad=12;
        if(precioMin<0)precioMin=0;if(precioMax<0)precioMax=null;if(precioMin.HasValue&&precioMax.HasValue&&precioMin>precioMax)(precioMin,precioMax)=(precioMax,precioMin);
        int total=repositorio.Contar(busqueda,tipo,precioMin,precioMax,soloDisponibles);int paginas=(int)Math.Ceiling(total/(double)cantidad);if(paginas>0&&pagina>paginas)pagina=paginas;
        return View(new CatalogoViewModel{Productos=repositorio.Buscar(busqueda,tipo,precioMin,precioMax,soloDisponibles,ordenar,pagina,cantidad),Busqueda=busqueda,Tipo=tipo,PrecioMin=precioMin,PrecioMax=precioMax,SoloDisponibles=soloDisponibles,Ordenar=ordenar,Pagina=pagina,TotalPaginas=paginas,Total=total});
    }
}
