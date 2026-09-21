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

    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult EliminarSeleccionados(List<int>? ids)
    {
        int eliminados=repositorio.EliminarDisenos(ClienteId(),ids??[]);
        TempData[eliminados>0?"Mensaje":"Error"]=eliminados>0?$"Eliminaste {eliminados} diseño{(eliminados==1?"":"s")}.":"Seleccioná al menos un diseño.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Renombrar(int id,string nombre)
    {
        nombre=(nombre??"").Trim();
        if(nombre.Length is < 2 or > 100){TempData["Error"]="El nombre debe tener entre 2 y 100 caracteres.";return RedirectToAction(nameof(Index));}
        bool actualizado=repositorio.RenombrarDiseno(id,ClienteId(),nombre);
        TempData[actualizado?"Mensaje":"Error"]=actualizado?"Nombre del diseño actualizado.":"No se encontró el diseño.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Duplicar(int id,string nombre)
    {
        nombre=(nombre??"").Trim();
        if(nombre.Length is < 2 or > 100){TempData["Error"]="El nombre de la copia no es válido.";return RedirectToAction(nameof(Index));}
        int? nuevoId=repositorio.DuplicarDiseno(id,ClienteId(),nombre);
        TempData[nuevoId.HasValue?"Mensaje":"Error"]=nuevoId.HasValue?"Diseño duplicado. Podés editar la copia sin modificar el original.":"No se encontró el diseño.";
        return RedirectToAction(nameof(Index));
    }
    private static bool JsonValido(string valor){try{using var _=JsonDocument.Parse(valor);return true;}catch(JsonException){return false;}}
    private int ClienteId()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out int id)?id:throw new UnauthorizedAccessException();
}
