using System.Security.Claims;
using ClassLibrary.Cursos;
using ClassLibrary.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models.Cursos;

namespace WebApplication2.Controllers;

public class CursosController(ICursoRepositorio repositorio) : Controller
{
    [Authorize(Roles="Shaper")]
    public IActionResult Index()=>View(repositorio.ObtenerPorShaper(UsuarioId(),false));

    [Authorize(Roles="Shaper")]
    [HttpGet] public IActionResult Crear()=>View("Formulario",new CursoFormModel());

    [Authorize(Roles="Shaper")]
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Crear(CursoFormModel modelo)
    {
        ValidarFecha(modelo); if(!ModelState.IsValid)return View("Formulario",modelo);
        var curso=Mapear(modelo);curso.ShaperId=UsuarioId();
        repositorio.Insertar(curso);TempData["Mensaje"]="Curso publicado correctamente.";return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles="Shaper")]
    [HttpGet] public IActionResult Editar(int id)
    {
        var c=repositorio.ObtenerPorId(id);if(c==null||c.ShaperId!=UsuarioId())return NotFound();
        return View("Formulario",new CursoFormModel{Id=c.Id,Titulo=c.Titulo,Resumen=c.Resumen,Descripcion=c.Descripcion,Modalidad=c.Modalidad,Ubicacion=c.Ubicacion,FechaInicio=c.FechaInicio.ToLocalTime(),DuracionHoras=c.DuracionHoras,Cupos=c.Cupos,Precio=c.Precio,ImagenUrl=c.ImagenUrl,Publicado=c.Publicado});
    }

    [Authorize(Roles="Shaper")]
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Editar(CursoFormModel modelo)
    {
        var existente=repositorio.ObtenerPorId(modelo.Id);if(existente==null||existente.ShaperId!=UsuarioId())return NotFound();
        ValidarFecha(modelo);if(modelo.Cupos<existente.CuposOcupados)ModelState.AddModelError(nameof(modelo.Cupos),$"Ya hay {existente.CuposOcupados} lugares reservados.");if(!ModelState.IsValid)return View("Formulario",modelo);
        var curso=Mapear(modelo);curso.Id=modelo.Id;repositorio.Actualizar(curso,UsuarioId());TempData["Mensaje"]="Curso actualizado.";return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles="Shaper")]
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult CambiarPublicacion(int id,bool publicado){if(!repositorio.CambiarPublicacion(id,UsuarioId(),publicado))return NotFound();TempData["Mensaje"]=publicado?"Curso publicado.":"Curso pausado.";return RedirectToAction(nameof(Index));}

    [Authorize(Roles="Shaper")]
    public IActionResult Inscripciones()=>View(repositorio.ObtenerInscripcionesDelShaper(UsuarioId()));

    [Authorize(Roles="Cliente,Shaper")]
    public IActionResult Detalle(int id)
    {
        var curso=repositorio.ObtenerPorId(id);if(curso==null)return NotFound();
        bool propietario=User.IsInRole("Shaper")&&UsuarioId()==curso.ShaperId;if(!curso.Publicado&&!propietario)return NotFound();ViewBag.EsPropietario=propietario;return View(curso);
    }

    [Authorize(Roles="Cliente")]
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Inscribirse(int id){bool ok=repositorio.CrearInscripcion(id,UsuarioId(),out string mensaje);TempData[ok?"Mensaje":"Error"]=mensaje;return RedirectToAction(nameof(Detalle),new{id});}

    private int UsuarioId()=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private void ValidarFecha(CursoFormModel m){if(m.FechaInicio<=DateTime.Now)ModelState.AddModelError(nameof(m.FechaInicio),"La fecha debe ser futura.");if(m.Modalidad=="Online"&&string.IsNullOrWhiteSpace(m.Ubicacion))ModelState.AddModelError(nameof(m.Ubicacion),"Indicá cómo se enviará el acceso.");}
    private CursoShaper Mapear(CursoFormModel m)=>new(){Titulo=m.Titulo.Trim(),Resumen=m.Resumen.Trim(),Descripcion=m.Descripcion.Trim(),Modalidad=m.Modalidad,Ubicacion=m.Ubicacion.Trim(),FechaInicio=m.FechaInicio.ToUniversalTime(),DuracionHoras=m.DuracionHoras,Cupos=m.Cupos,Precio=m.Precio,ImagenUrl=string.IsNullOrWhiteSpace(m.ImagenUrl)?null:m.ImagenUrl.Trim(),Publicado=m.Publicado};
}
