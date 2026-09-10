using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class SurfController : Controller
    {
        private readonly ClassLibrary.Servicios.IUsuarioServicio _usuarioServicio;
        private readonly ClassLibrary.Servicios.IProductoServicio _productoServicio;
        private readonly ClassLibrary.Servicios.IPuntoRetiroServicio _puntoRetiroServicio;
        private readonly WebApplication2.Servicios.ICorreoNotificacionServicio _correo;

        public SurfController(
            ClassLibrary.Servicios.IUsuarioServicio usuarioServicio,
            ClassLibrary.Servicios.IProductoServicio productoServicio,
            ClassLibrary.Servicios.IPuntoRetiroServicio puntoRetiroServicio,
            WebApplication2.Servicios.ICorreoNotificacionServicio correo)
        {
            _usuarioServicio = usuarioServicio;
            _productoServicio = productoServicio;
            _puntoRetiroServicio = puntoRetiroServicio;
            _correo = correo;
        }

        public IActionResult carrito() { return View(); }
        public IActionResult Dealers()
        {
            return View(_puntoRetiroServicio.ObtenerActivos());
        }
        public IActionResult Home()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction(
                        "Index",
                        "PanelAdmin"
                    );
                }

                if (User.IsInRole("Shaper"))
                {
                    return RedirectToAction(
                        "Index",
                        "Dashboard"
                    );
                }
            }

            return View();
        }
        public IActionResult master() { return View(); }
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Cliente")]
        public IActionResult shapers()
        {
            return View(CrearCatalogoShapers());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Cliente")]
        public async Task<IActionResult> SolicitarIngresoShaper(WebApplication2.Models.UnirseShaperViewModel solicitud)
        {
            var modelo = CrearCatalogoShapers();
            modelo.Solicitud = solicitud;
            if (!ModelState.IsValid)
            {
                ViewBag.AbrirSolicitudShaper = true;
                return View("shapers", modelo);
            }

            if (!await _correo.EnviarSolicitudShaperAsync(solicitud))
            {
                ModelState.AddModelError(string.Empty, "No pudimos enviar tu solicitud en este momento. Intentá nuevamente.");
                ViewBag.AbrirSolicitudShaper = true;
                return View("shapers", modelo);
            }

            TempData["SolicitudShaperEnviada"] = "Recibimos tu solicitud. El equipo de Zephyr Surf Go se comunicará contigo.";
            return RedirectToAction(nameof(shapers), new { enviado = true });
        }

        private WebApplication2.Models.ShapersCatalogoViewModel CrearCatalogoShapers()
        {
            var modelo = new WebApplication2.Models.ShapersCatalogoViewModel();

            foreach (var shaper in _usuarioServicio.ObtenerShapers())
            {
                if (!shaper.Activo) continue;

                modelo.Shapers.Add(new WebApplication2.Models.ShaperCatalogoItemViewModel
                {
                    Shaper = shaper,
                    CantidadProductos = _productoServicio.BuscarPorShaper(shaper.Id).Count
                });
            }

            return modelo;
        }
    }
}
