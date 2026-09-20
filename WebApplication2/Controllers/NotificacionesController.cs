using ClassLibrary.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace WebApplication2.Controllers;
[Authorize(Roles="Cliente,Shaper")]
public class NotificacionesController(INotificacionRepositorio repositorio):Controller
{
 public IActionResult Index()=>View(repositorio.Obtener(UsuarioId()));
 [HttpPost,ValidateAntiForgeryToken]public IActionResult Leer(long id,string? volver=null){repositorio.MarcarLeida(id,UsuarioId());return !string.IsNullOrWhiteSpace(volver)&&Url.IsLocalUrl(volver)?LocalRedirect(volver):RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken]public IActionResult LeerTodas(){repositorio.MarcarTodasLeidas(UsuarioId());return RedirectToAction(nameof(Index));}
 private int UsuarioId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out int id)?id:throw new UnauthorizedAccessException();
}
