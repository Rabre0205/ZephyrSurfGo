using ClassLibrary.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers;

[Authorize(Roles="Cliente")]
public class FavoritosController(IInteraccionesRepositorio repositorio) : Controller
{
    public IActionResult Index() => View(repositorio.ObtenerFavoritos(ClienteId()));

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Alternar(int productoId, string? volverA)
    {
        bool guardado = repositorio.AlternarFavorito(ClienteId(), productoId);
        TempData["Mensaje"] = guardado ? "Producto guardado en tus favoritos." : "Producto eliminado de tus favoritos.";
        return LocalRedirect(!string.IsNullOrWhiteSpace(volverA) && Url.IsLocalUrl(volverA) ? volverA : Url.Action(nameof(Index))!);
    }

    private int ClienteId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id)
        ? id : throw new UnauthorizedAccessException();
}
