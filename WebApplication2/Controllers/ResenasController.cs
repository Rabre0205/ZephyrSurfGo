using ClassLibrary.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles="Cliente")]
public class ResenasController(IInteraccionesRepositorio repositorio) : Controller
{
    [HttpGet,Authorize(Roles="Cliente")]
    public IActionResult Crear(int pedidoId, int productoId)
    {
        if (!repositorio.PuedeResenar(ClienteId(), pedidoId, productoId)) return Forbid();
        ViewBag.PedidoId=pedidoId; ViewBag.ProductoId=productoId;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken,Authorize(Roles="Cliente")]
    public IActionResult Crear(int pedidoId, int productoId, byte estrellas, string comentario)
    {
        comentario=(comentario??"").Trim();
        if(estrellas is < 1 or > 5) ModelState.AddModelError("estrellas","Elegí entre 1 y 5 estrellas.");
        if(comentario.Length<3 || comentario.Length>1000) ModelState.AddModelError("comentario","El comentario debe tener entre 3 y 1000 caracteres.");
        if(!ModelState.IsValid){ViewBag.PedidoId=pedidoId;ViewBag.ProductoId=productoId;return View();}
        if(!repositorio.GuardarResena(ClienteId(),pedidoId,productoId,estrellas,comentario)){
            TempData["Error"]="Solo podés reseñar una vez un producto de una compra completada.";
        } else TempData["Mensaje"]="Gracias. Tu reseña fue publicada.";
        return RedirectToAction("Detalle","MisPedidos",new{id=pedidoId});
    }

    [AllowAnonymous]
    public IActionResult Producto(int id) => View(repositorio.ObtenerResenas(id));
    [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Cliente")]
    public IActionResult Editar(int id,int productoId,byte estrellas,string comentario){comentario=(comentario??"").Trim();if(estrellas is <1 or >5||comentario.Length is <3 or >1000){TempData["Error"]="Revisá la puntuación y el comentario.";}else if(!repositorio.EditarResena(id,ClienteId(),estrellas,comentario))TempData["Error"]="No se pudo editar la reseña.";else TempData["Mensaje"]="Reseña actualizada.";return RedirectToAction(nameof(Producto),new{id=productoId});}
    private int ClienteId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out int id)?id:throw new UnauthorizedAccessException();
}
