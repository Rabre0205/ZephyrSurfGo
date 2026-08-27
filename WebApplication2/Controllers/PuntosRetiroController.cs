using ClassLibrary.PuntosRetiro;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles="Shaper")]
public class PuntosRetiroController : Controller
{
    private readonly IPuntoRetiroServicio _servicio;
    public PuntosRetiroController(IPuntoRetiroServicio servicio)=>_servicio=servicio;
    public IActionResult Index()=>View(_servicio.ObtenerPorShaper(UsuarioId()));
    [HttpGet] public IActionResult Crear()=>View("Formulario",new PuntoRetiro());
    [HttpGet] public IActionResult Editar(int id){var p=_servicio.ObtenerParaEditar(id,UsuarioId());return p==null?NotFound():View("Formulario",p);}
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Guardar(PuntoRetiro modelo)
    {
        var resultado=_servicio.Guardar(modelo,UsuarioId());
        if(!resultado.Exito){ModelState.AddModelError("",resultado.Error);return View("Formulario",modelo);}
        TempData["Mensaje"]="El punto de retiro se guardó correctamente.";return RedirectToAction(nameof(Index));
    }
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id,bool activo)
    {
        bool actualizado=_servicio.CambiarEstado(id,UsuarioId(),activo);
        TempData[actualizado?"Mensaje":"Error"]=actualizado
            ? (activo?"El punto vuelve a aparecer en el mapa.":"El punto fue ocultado del mapa.")
            : "No se pudo modificar el punto de retiro.";
        return RedirectToAction(nameof(Index));
    }
    private int UsuarioId()=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
