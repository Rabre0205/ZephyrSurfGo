using ClassLibrary;
using ClassLibrary.Enums;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models.Productos;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Shaper")]
    public class AdminController : Controller
    {
        private readonly IProductoServicio _productoServicio;
        private readonly IUsuarioServicio _usuarioServicio;

        public AdminController(IProductoServicio productoServicio, IUsuarioServicio usuarioServicio)
        {
            _productoServicio = productoServicio;
            _usuarioServicio = usuarioServicio;
        }

        public IActionResult AgregarTabla()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarTabla(TablaFormModel modelo)
        {
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

            if (!int.TryParse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out int shaperId))
            {
                return Unauthorized();
            }

            int idGenerado;
            try
            {
                idGenerado = _productoServicio.AgregarTabla(
                    titulo: modelo.Titulo,
                    subtitulo: modelo.Subtitulo,
                    precio: modelo.Precio,
                    descripcion: modelo.Descripcion,
                    //shaperId: shaperId.Value,
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
                    imagenTrasera: modelo.ImagenTrasera);
            }
            catch (Exception)
            {
                ViewBag.Error = "Ocurrió un error al guardar la tabla. Intentá nuevamente.";
                return View(modelo);
            }

            TempData["Mensaje"] = "Tabla agregada correctamente.";
            return View();
        }


        [HttpGet]
        public IActionResult RegistrarCliente()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarCliente(RegistrarClienteFormModel modelo)
        {
            var resultado = _usuarioServicio.RegistrarCliente(
                modelo.Email,
                modelo.Nombre,
                (Pais)modelo.Pais,
                modelo.Contrasenia,
                modelo.ConfirmarContrasenia);

            if (!resultado.Exito)
            {
                ViewBag.Error = resultado.Error;
                return View(modelo);
            }

            TempData["Mensaje"] = "Cliente registrado correctamente.";
            return RedirectToAction("RegistrarCliente");
        }
    }

}
