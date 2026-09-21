using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles="Shaper")]
public class AyudaController : Controller
{
    private readonly ISolicitudSoporteServicio _servicio;
    private readonly WebApplication2.Servicios.ICorreoNotificacionServicio _correo;
    public AyudaController(ISolicitudSoporteServicio servicio, WebApplication2.Servicios.ICorreoNotificacionServicio correo)
    { _servicio=servicio; _correo=correo; }
    public IActionResult Index()=>View(_servicio.ObtenerPorShaper(UsuarioId()));
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(string asunto,string mensaje)
    {
        var r=_servicio.Crear(UsuarioId(),asunto,mensaje);
        if(!r.Exito) TempData["Error"]=r.Error;
        else
        {
            var consulta=_servicio.ObtenerPorId(r.Id);
            bool notificada=consulta!=null && await _correo.EnviarConsultaSoporteAsync(consulta);
            TempData["Mensaje"]=notificada
                ? "Recibimos tu consulta y notificamos al equipo por correo. Podés seguir la respuesta desde esta página."
                : "Recibimos tu consulta y quedó registrada. No se pudo enviar la notificación por correo; el equipo igualmente podrá verla en el panel.";
        }
        return RedirectToAction(nameof(Index));
    }
    public IActionResult Detalle(int id){var s=_servicio.ObtenerParaShaper(id,UsuarioId());return s==null?NotFound():View(s);}
    private int UsuarioId()=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
