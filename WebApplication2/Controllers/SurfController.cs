using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class SurfController : Controller
    {
        private readonly ClassLibrary.Servicios.IUsuarioServicio _usuarioServicio;
        private readonly ClassLibrary.Servicios.IProductoServicio _productoServicio;
        private readonly ClassLibrary.Servicios.IPuntoRetiroServicio _puntoRetiroServicio;

        public SurfController(
            ClassLibrary.Servicios.IUsuarioServicio usuarioServicio,
            ClassLibrary.Servicios.IProductoServicio productoServicio,
            ClassLibrary.Servicios.IPuntoRetiroServicio puntoRetiroServicio)
        {
            _usuarioServicio = usuarioServicio;
            _productoServicio = productoServicio;
            _puntoRetiroServicio = puntoRetiroServicio;
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

            return View(modelo);
        }
    }
}
