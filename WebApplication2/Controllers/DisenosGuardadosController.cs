using ClassLibrary.Datos;
using ClassLibrary.Interacciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace WebApplication2.Controllers;

[Authorize(Roles="Cliente")]
public class DisenosGuardadosController(IInteraccionesRepositorio repositorio) : Controller
{
    public IActionResult Index()=>View(repositorio.ObtenerDisenos(ClienteId()));

    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Guardar(int shaperId,string nombre,string configuracionJson)
    {
        nombre=(nombre??"").Trim(); configuracionJson??="";
        if(shaperId <= 0 || nombre.Length is < 2 or > 100 || configuracionJson.Length>15000 || !JsonValido(configuracionJson))
            return BadRequest(new{guardado=false,mensaje="El nombre o la configuración del diseño no son válidos."});
        int id=repositorio.GuardarDiseno(new(){ClienteId=ClienteId(),ShaperId=shaperId,Nombre=nombre,ConfiguracionJson=configuracionJson});
        return Json(new{guardado=true,id,mensaje="Diseño guardado para continuar más adelante."});
    }

    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id){repositorio.EliminarDiseno(id,ClienteId());return RedirectToAction(nameof(Index));}
    private static bool JsonValido(string valor){try{using var _=JsonDocument.Parse(valor);return true;}catch(JsonException){return false;}}
    private int ClienteId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out int id)?id:throw new UnauthorizedAccessException();
}
