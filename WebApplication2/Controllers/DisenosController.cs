using ClassLibrary.Disenos;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles="Shaper")]
public class DisenosController : Controller
{
    private readonly IDisenoShaperServicio _servicio;
    private readonly ICloudinaryServicio _imagenes;
    public DisenosController(IDisenoShaperServicio servicio,ICloudinaryServicio imagenes){_servicio=servicio;_imagenes=imagenes;}
    public IActionResult Index()=>View(_servicio.ObtenerPorShaper(UsuarioId()));
    [HttpGet] public IActionResult Crear()=>View("Formulario",new DisenoShaper());
    [HttpGet] public IActionResult Editar(int id){var d=_servicio.ObtenerParaEditar(id,UsuarioId());return d==null?NotFound():View("Formulario",d);}

    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Guardar(DisenoShaper modelo,IFormFile? imagen)
    {
        if(imagen is {Length:>0})
        {
            if(imagen.Length>5*1024*1024||!new[]{"image/jpeg","image/png","image/webp"}.Contains(imagen.ContentType))
            {ModelState.AddModelError("","La imagen debe ser JPG, PNG o WEBP y pesar menos de 5 MB.");return View("Formulario",modelo);}
            modelo.ImagenUrl=_imagenes.SubirImagen(imagen,$"zephyr/disenos/{UsuarioId()}-{Guid.NewGuid():N}");
        }
        var resultado=_servicio.Guardar(modelo,UsuarioId());
        if(!resultado.Exito){ModelState.AddModelError("",resultado.Error);return View("Formulario",modelo);}
        TempData["Mensaje"]="El diseño se guardó correctamente.";return RedirectToAction(nameof(Index));
    }

    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id,bool activo)
    {
        bool ok=_servicio.CambiarEstado(id,UsuarioId(),activo);
        TempData[ok?"Mensaje":"Error"]=ok?(activo?"El diseño vuelve a estar disponible.":"El diseño fue ocultado."):"No se pudo modificar el diseño.";
        return RedirectToAction(nameof(Index));
    }
    private int UsuarioId()=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
