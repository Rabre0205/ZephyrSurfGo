using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers;

[Authorize(Roles="Administrador")]
public class SoporteAdminController : Controller
{
    private readonly ISolicitudSoporteServicio _servicio;
    public SoporteAdminController(ISolicitudSoporteServicio servicio)=>_servicio=servicio;
    public IActionResult Index()=>View(_servicio.ObtenerTodas());
    public IActionResult Detalle(int id){var s=_servicio.ObtenerPorId(id);return s==null?NotFound():View(s);}
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Responder(int id,string respuesta,bool cerrar)
    {
        bool actualizado=_servicio.Responder(id,respuesta,cerrar);
        TempData[actualizado?"Mensaje":"Error"]=actualizado?"La respuesta fue guardada.":"Escribí una respuesta antes de guardar.";
        return RedirectToAction(nameof(Detalle),new{id});
    }
}
