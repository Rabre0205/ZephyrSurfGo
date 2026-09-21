using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class SurfController : Controller
    {
        private readonly ClassLibrary.Servicios.IUsuarioServicio _usuarioServicio;
        private readonly ClassLibrary.Servicios.IProductoServicio _productoServicio;
        private readonly ClassLibrary.Servicios.IPuntoRetiroServicio _puntoRetiroServicio;
        private readonly WebApplication2.Servicios.ICorreoNotificacionServicio _correo;
        private readonly ClassLibrary.Datos.ISolicitudShaperRepositorio? _solicitudesShapers;

        public SurfController(
            ClassLibrary.Servicios.IUsuarioServicio usuarioServicio,
            ClassLibrary.Servicios.IProductoServicio productoServicio,
            ClassLibrary.Servicios.IPuntoRetiroServicio puntoRetiroServicio,
            WebApplication2.Servicios.ICorreoNotificacionServicio correo, ClassLibrary.Datos.ISolicitudShaperRepositorio? solicitudesShapers=null)
        {
            _usuarioServicio = usuarioServicio;
            _productoServicio = productoServicio;
            _puntoRetiroServicio = puntoRetiroServicio;
            _correo = correo;
            _solicitudesShapers=solicitudesShapers;
        }

        public IActionResult carrito() { return RedirectToAction("Index", "Carrito"); }
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
        public IActionResult shapers()
        {
            return View(CrearCatalogoShapers());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarIngresoShaper(WebApplication2.Models.UnirseShaperViewModel solicitud)
        {
            var modelo = CrearCatalogoShapers();
            modelo.Solicitud = solicitud;
            if (!ModelState.IsValid)
            {
                ViewBag.AbrirSolicitudShaper = true;
                return View("shapers", modelo);
            }

            if(_solicitudesShapers==null){ModelState.AddModelError(string.Empty,"No pudimos registrar tu solicitud.");ViewBag.AbrirSolicitudShaper=true;return View("shapers",modelo);}
            _solicitudesShapers.Crear(new ClassLibrary.Solicitudes.SolicitudShaper{Nombre=solicitud.Nombre.Trim(),Email=solicitud.Email.Trim(),Marca=solicitud.Marca.Trim(),Ubicacion=solicitud.Ubicacion.Trim(),Celular=solicitud.Celular.Trim(),Instagram=solicitud.Instagram.Trim(),AniosExperiencia=solicitud.AniosExperiencia,RealizaPersonalizadas=solicitud.RealizaPersonalizadas,RealizaEnvios=solicitud.RealizaEnvios,Presentacion=solicitud.Presentacion.Trim()});
            bool correoEnviado=await _correo.EnviarSolicitudShaperAsync(solicitud);

            TempData["SolicitudShaperEnviada"] = correoEnviado?"Recibimos tu solicitud. El equipo de Zephyr Surf Go se comunicará contigo.":"Recibimos y guardamos tu solicitud. La notificación por correo quedó pendiente.";
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
