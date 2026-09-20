using ClassLibrary.Datos;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using System.Security.Claims;
namespace WebApplication2.Controllers;
[Authorize(Roles="Shaper")]
public class ShaperResenasController(IInteraccionesRepositorio repositorio, WebApplication2.Servicios.ICorreoNotificacionServicio? correo=null):Controller
{
 [HttpPost,ValidateAntiForgeryToken]public IActionResult Responder(int id,int productoId,string respuesta){respuesta=(respuesta??"").Trim();if(respuesta.Length is <2 or >1000)TempData["Error"]="La respuesta debe tener entre 2 y 1000 caracteres.";else if(!repositorio.ResponderResena(id,ShaperId(),respuesta))return Forbid();else{TempData["Mensaje"]="Respuesta publicada.";var resena=repositorio.ObtenerResena(id);if(resena!=null&&correo!=null)correo.EnviarAUsuarioAsync(resena.ClienteId,$"Respondieron tu reseña de {resena.ProductoTitulo}","El shaper respondió tu reseña",respuesta).GetAwaiter().GetResult();}return RedirectToAction("Producto","Resenas",new{id=productoId});}
 private int ShaperId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out int id)?id:throw new UnauthorizedAccessException();
}
