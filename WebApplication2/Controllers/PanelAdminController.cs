using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models.PanelAdmin;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PanelAdminController : Controller
    {
        private readonly IUsuarioServicio _usuarioServicio;

        public PanelAdminController(
            IUsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        public IActionResult Index()
        {
            var shapers = _usuarioServicio.ObtenerShapers();

            ViewBag.CantidadShapers = shapers.Count;

            return View();
        }

        public IActionResult Shapers()
        {
            var shapers = _usuarioServicio.ObtenerShapers();

            return View(shapers);
        }

        [HttpGet]
        public IActionResult RegistrarShaper()
        {
            return View(new RegistrarShaperViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarShaper(
            RegistrarShaperViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = _usuarioServicio.RegistrarShaper(
                modelo.Email.Trim(),
                modelo.Nombre.Trim(),
                modelo.Pais,
                modelo.Contrasenia,
                modelo.ConfirmarContrasenia,
                modelo.NombreDeNegosio.Trim(),
                modelo.Contacto.Trim()
            );

            if (!resultado.Exito)
            {
                ModelState.AddModelError(
                    string.Empty,
                    resultado.Error
                );

                return View(modelo);
            }

            TempData["Mensaje"] =
                "El shaper fue registrado correctamente.";

            return RedirectToAction(nameof(Shapers));
        }

        public IActionResult Productos()
        {
            return View();
        }

        public IActionResult Pedidos()
        {
            return View();
        }

        public IActionResult Estadisticas()
        {
            return View();
        }
    }
}