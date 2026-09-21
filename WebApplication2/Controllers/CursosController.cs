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
    [HttpGet] public IActionResult Crear()=>View("Formulario",new CursoFormModel{Modulos=new(){new CursoModuloFormModel{Nombre="Módulo 1",Encuentros=1}}});

    [Authorize(Roles="Shaper")]
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Crear(CursoFormModel modelo)
    {
        ValidarModelo(modelo); if(!ModelState.IsValid)return View("Formulario",modelo);
        modelo.Modulos.ForEach(x=>x.Id=0);var curso=Mapear(modelo);curso.ShaperId=UsuarioId();
        repositorio.Insertar(curso);TempData["Mensaje"]="Curso publicado correctamente.";return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles="Shaper")]
    [HttpGet] public IActionResult Editar(int id)
    {
        var c=repositorio.ObtenerPorId(id);if(c==null||c.ShaperId!=UsuarioId())return NotFound();
        return View("Formulario",new CursoFormModel{Id=c.Id,Titulo=c.Titulo,Resumen=c.Resumen,Descripcion=c.Descripcion,Modalidad=c.Modalidad,Ubicacion=c.Ubicacion,FechaInicio=c.FechaInicio.ToLocalTime(),DuracionHoras=c.DuracionHoras,Cupos=c.Cupos,Precio=c.Precio,ImagenUrl=c.ImagenUrl,InstructorNombre=c.InstructorNombre,InstructorBio=c.InstructorBio,PublicoObjetivo=c.PublicoObjetivo,Incluye=c.Incluye,Requisitos=c.Requisitos,CronogramaUrl=c.CronogramaUrl,Contacto=c.Contacto,CuotasMaximas=c.CuotasMaximas,DescuentoAcompanantePorcentaje=c.DescuentoAcompanantePorcentaje,HospedajeDisponible=c.HospedajeDisponible,GaleriaUrls=string.Join(Environment.NewLine,c.Imagenes),Publicado=c.Publicado,Modulos=c.Modulos.Select(m=>new CursoModuloFormModel{Id=m.Id,Nombre=m.Nombre,Descripcion=m.Descripcion,Encuentros=m.Encuentros,DuracionHoras=m.DuracionHoras,Precio=m.Precio,PermiteCompraIndividual=m.PermiteCompraIndividual}).ToList()});
    }

    [Authorize(Roles="Shaper")]
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Editar(CursoFormModel modelo)
    {
        var existente=repositorio.ObtenerPorId(modelo.Id);if(existente==null||existente.ShaperId!=UsuarioId())return NotFound();if(modelo.Modulos.Any(x=>x.Id>0&&existente.Modulos.All(m=>m.Id!=x.Id)))return BadRequest();
        ValidarModelo(modelo);if(modelo.Cupos<existente.CuposOcupados)ModelState.AddModelError(nameof(modelo.Cupos),$"Ya hay {existente.CuposOcupados} lugares reservados.");if(!ModelState.IsValid)return View("Formulario",modelo);
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
    public IActionResult Inscribirse(int id,int? moduloId){bool ok=repositorio.CrearInscripcion(id,UsuarioId(),moduloId,out string mensaje);TempData[ok?"Mensaje":"Error"]=mensaje;return RedirectToAction(nameof(Detalle),new{id});}

    private int UsuarioId()=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private void ValidarModelo(CursoFormModel m){if(m.FechaInicio<=DateTime.Now)ModelState.AddModelError(nameof(m.FechaInicio),"La fecha debe ser futura.");if(m.Modalidad=="Online"&&string.IsNullOrWhiteSpace(m.Ubicacion))ModelState.AddModelError(nameof(m.Ubicacion),"Indicá cómo se enviará el acceso.");if(m.Modulos.Count==0)ModelState.AddModelError(nameof(m.Modulos),"Agregá al menos un módulo.");for(int i=0;i<m.Modulos.Count;i++){if(m.Modulos[i].PermiteCompraIndividual&&!m.Modulos[i].Precio.HasValue)ModelState.AddModelError($"Modulos[{i}].Precio","Indicá el precio del módulo o desactivá la compra individual.");}foreach(string url in SepararImagenes(m.GaleriaUrls)){if(!Uri.TryCreate(url,UriKind.Absolute,out var uri)||uri.Scheme is not ("http" or "https")){ModelState.AddModelError(nameof(m.GaleriaUrls),$"La URL de galería '{url}' no es válida.");break;}}}
    private CursoShaper Mapear(CursoFormModel m)=>new(){Titulo=m.Titulo.Trim(),Resumen=m.Resumen.Trim(),Descripcion=m.Descripcion.Trim(),Modalidad=m.Modalidad,Ubicacion=m.Ubicacion.Trim(),FechaInicio=m.FechaInicio.ToUniversalTime(),DuracionHoras=m.DuracionHoras,Cupos=m.Cupos,Precio=m.Precio,ImagenUrl=string.IsNullOrWhiteSpace(m.ImagenUrl)?null:m.ImagenUrl.Trim(),InstructorNombre=m.InstructorNombre.Trim(),InstructorBio=m.InstructorBio.Trim(),PublicoObjetivo=m.PublicoObjetivo.Trim(),Incluye=m.Incluye.Trim(),Requisitos=m.Requisitos.Trim(),CronogramaUrl=string.IsNullOrWhiteSpace(m.CronogramaUrl)?null:m.CronogramaUrl.Trim(),Contacto=string.IsNullOrWhiteSpace(m.Contacto)?null:m.Contacto.Trim(),CuotasMaximas=m.CuotasMaximas,DescuentoAcompanantePorcentaje=m.DescuentoAcompanantePorcentaje,HospedajeDisponible=m.HospedajeDisponible,Imagenes=SepararImagenes(m.GaleriaUrls).Distinct().Take(12).ToList(),Publicado=m.Publicado,Modulos=m.Modulos.Select((x,i)=>new CursoModulo{Id=x.Id,Nombre=x.Nombre.Trim(),Descripcion=x.Descripcion.Trim(),Encuentros=x.Encuentros,DuracionHoras=x.DuracionHoras,Precio=x.Precio,PermiteCompraIndividual=x.PermiteCompraIndividual,Orden=i}).ToList()};
    private static IEnumerable<string> SepararImagenes(string? urls)=>(urls??"").Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
}
