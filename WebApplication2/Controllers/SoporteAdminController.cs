using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers;

[Authorize(Roles="Administrador")]
public class SoporteAdminController : Controller
{
    private readonly ISolicitudSoporteServicio _servicio;
    private readonly WebApplication2.Servicios.ICorreoNotificacionServicio? _correo;
    public SoporteAdminController(ISolicitudSoporteServicio servicio, WebApplication2.Servicios.ICorreoNotificacionServicio? correo=null){_servicio=servicio;_correo=correo;}
    public IActionResult Index()=>View(_servicio.ObtenerTodas());
    public IActionResult Detalle(int id){var s=_servicio.ObtenerPorId(id);return s==null?NotFound():View(s);}
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(int id,string respuesta,bool cerrar)
    {
        bool actualizado=_servicio.Responder(id,respuesta,cerrar);
        if(actualizado&&_correo!=null)
        {
            var consulta=_servicio.ObtenerPorId(id);
            if(consulta!=null) await _correo.EnviarAUsuarioAsync(consulta.ShaperId,$"Respuesta a tu consulta #{id}","El equipo respondió tu consulta",respuesta);
        }
        TempData[actualizado?"Mensaje":"Error"]=actualizado?"La respuesta fue guardada.":"Escribí una respuesta antes de guardar.";
        return RedirectToAction(nameof(Detalle),new{id});
    }
}
