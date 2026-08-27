using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles="Shaper")]
public class AyudaController : Controller
{
    private readonly ISolicitudSoporteServicio _servicio;
    public AyudaController(ISolicitudSoporteServicio servicio)=>_servicio=servicio;
    public IActionResult Index()=>View(_servicio.ObtenerPorShaper(UsuarioId()));
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Crear(string asunto,string mensaje)
    {
        var r=_servicio.Crear(UsuarioId(),asunto,mensaje);
        TempData[r.Exito?"Mensaje":"Error"]=r.Exito?"Recibimos tu consulta. Podés seguir la respuesta desde esta página.":r.Error;
        return RedirectToAction(nameof(Index));
    }
    public IActionResult Detalle(int id){var s=_servicio.ObtenerParaShaper(id,UsuarioId());return s==null?NotFound():View(s);}
    private int UsuarioId()=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
