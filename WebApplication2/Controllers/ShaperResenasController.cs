using ClassLibrary.Datos;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using System.Security.Claims;
namespace WebApplication2.Controllers;
[Authorize(Roles="Shaper")]
public class ShaperResenasController(IInteraccionesRepositorio repositorio):Controller
{
 [HttpPost,ValidateAntiForgeryToken]public IActionResult Responder(int id,int productoId,string respuesta){respuesta=(respuesta??"").Trim();if(respuesta.Length is <2 or >1000)TempData["Error"]="La respuesta debe tener entre 2 y 1000 caracteres.";else if(!repositorio.ResponderResena(id,ShaperId(),respuesta))return Forbid();else TempData["Mensaje"]="Respuesta publicada.";return RedirectToAction("Producto","Resenas",new{id=productoId});}
 private int ShaperId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out int id)?id:throw new UnauthorizedAccessException();
}
