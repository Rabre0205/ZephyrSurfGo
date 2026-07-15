using ClassLibrary;
using ClassLibrary.Enums;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models.Productos;

namespace WebApplication2.Controllers
{
    public class AdminController : Controller
    {

        Sistema misistema = Sistema.ObtenerInstancia();
        
        public IActionResult AgregarTabla()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AgregarTabla(TablaFormModel modelo)
        {

            //Pasar Validaciones a un metodo en clase Tabla
            if (modelo == null)
            {
                ViewBag.Error = "No se recibieron datos del formulario.";
                return Redirect("admin/admin");
            }

            if (modelo.ImagenFrontal == null || modelo.ImagenFrontal.Length == 0)
            {
                ViewBag.Error = "La imagen frontal es obligatoria.";
                return View(modelo);
            }

            if (modelo.Precio <= 0)
            {
                ViewBag.Error = "El precio debe ser mayor a 0.";
                return View(modelo);
            }
            if (modelo.PesoMinimo <= 0)
            {
                ViewBag.Error = "El peso mínimo debe ser mayor a 0.";
                return View(modelo);
            }

            if (modelo.PesoMaximo < modelo.PesoMinimo)
            {
                ViewBag.Error = "El peso máximo no puede ser menor al peso mínimo.";
                return View(modelo);
            }

            // El ShaperId nunca se toma del formulario (evita que alguien lo manipule
            // desde el HTML): siempre se usa el usuario logueado en el singleton.

            misistema.UsuarioLogueado = misistema.Usuarios[0];
            int shaperId = misistema.UsuarioLogueado.Id;
            


            Sistema sistema = Sistema.ObtenerInstancia();

            int idGenerado = sistema.AgregarTabla(
                titulo: modelo.Titulo,
                subtitulo: modelo.Subtitulo,
                precio: modelo.Precio,
                descripcion: modelo.Descripcion,
                shaperId: shaperId,
                altura: modelo.Altura,
                ancho: modelo.Ancho,
                volumen: modelo.Volumen,
                sistemaDeEncaje: (SistemaDeEncaje)modelo.SistemaDeEncaje,
                tipoDeOla: (TipoDeOla)modelo.TipoDeOla,
                estiloDeSurf: (EstiloDeSurf)modelo.EstiloDeSurf,
                pesoMinimo: modelo.PesoMinimo,
                pesoMaximo: modelo.PesoMaximo,
                experiencia: (Experiencia)modelo.Experiencia,
                imagenFrontal: modelo.ImagenFrontal,
                imagenTrasera: modelo.ImagenTrasera
            );

            if (idGenerado == -1)
            {
                ViewBag.Error = "Ocurrió un error al guardar la tabla. Intentá nuevamente.";
                return View(modelo);
            }

            TempData["Mensaje"] = "Tabla agregada correctamente.";
            return View();
        }
    }
}
