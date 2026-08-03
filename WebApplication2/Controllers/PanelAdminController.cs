using ClassLibrary.Persona;
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

        public IActionResult Shapers(
    string busqueda = "",
    int pagina = 1)
        {
            const int cantidadPorPagina = 20;

            if (pagina < 1)
            {
                pagina = 1;
            }

            string textoBusqueda =
                busqueda?.Trim() ?? string.Empty;

            int totalResultados =
                _usuarioServicio.ContarShapers(
                    textoBusqueda
                );

            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)cantidadPorPagina
                );

            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            var shapers =
                _usuarioServicio.ObtenerShapersPaginados(
                    textoBusqueda,
                    pagina,
                    cantidadPorPagina
                );

            var modelo =
                new ShapersAdminViewModel
                {
                    Shapers = shapers,
                    Busqueda = textoBusqueda,
                    PaginaActual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalResultados = totalResultados
                };

            return View(modelo);
        }

        [HttpGet]
        public IActionResult EditarShaper(int id)
        {
            Shaper shaper = _usuarioServicio.ObtenerShaperPorId(id);

            if (shaper == null)
            {
                return NotFound();
            }

            EditarShaperViewModel modelo = new EditarShaperViewModel
            {
                Id = shaper.Id,
                Nombre = shaper.Nombre,
                Email = shaper.Email,
                Pais = shaper.Pais,
                NombreDeNegosio = shaper.NombreDeNegosio,
                Contacto = shaper.Contacto
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarShaper(EditarShaperViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var resultado = _usuarioServicio.ActualizarShaper(
                modelo.Id,
                modelo.Email,
                modelo.Nombre,
                modelo.Pais,
                modelo.NombreDeNegosio,
                modelo.Contacto
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
                "Shaper actualizado correctamente.";

            return RedirectToAction(nameof(Shapers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstadoShaper(int id)
        {
            var shaper = _usuarioServicio.ObtenerShaperPorId(id);

            if (shaper == null)
            {
                TempData["Error"] = "No se encontró el shaper.";
                return RedirectToAction(nameof(Shapers));
            }

            bool nuevoEstado = !shaper.Activo;

            bool actualizado = _usuarioServicio.CambiarEstadoShaper(
                id,
                nuevoEstado
            );

            if (!actualizado)
            {
                TempData["Error"] =
                    "No se pudo modificar el estado del shaper.";
            }
            else
            {
                TempData["Mensaje"] = nuevoEstado
                    ? "El shaper fue activado correctamente."
                    : "El shaper fue desactivado correctamente.";
            }

            return RedirectToAction(nameof(Shapers));
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