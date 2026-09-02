using ClassLibrary.Enums;
using ClassLibrary.Persona;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class ShaperController : Controller
    {
        private readonly ClassLibrary.Servicios.IUsuarioServicio _usuarioServicio;
        private readonly ClassLibrary.Servicios.IProductoServicio _productoServicio;
        private readonly ClassLibrary.Servicios.IDisenoShaperServicio _disenoServicio;

        public ShaperController(
            ClassLibrary.Servicios.IUsuarioServicio usuarioServicio,
            ClassLibrary.Servicios.IProductoServicio productoServicio,
            ClassLibrary.Servicios.IDisenoShaperServicio disenoServicio)
        {
            _usuarioServicio = usuarioServicio;
            _productoServicio = productoServicio;
            _disenoServicio = disenoServicio;
        }

        public IActionResult Detalle(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Challenge();
            }

            if (User.IsInRole("Shaper"))
            {
                string? usuarioId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(usuarioId, out int shaperAutenticadoId) ||
                    shaperAutenticadoId != id)
                {
                    return Forbid();
                }
            }
            else if (!User.IsInRole("Cliente"))
            {
                return Forbid();
            }

            Shaper? shaper = _usuarioServicio.ObtenerShaperPorId(id);

            if (shaper == null || !shaper.Activo)
            {
                return NotFound();
            }

            ShaperDetalleViewModel modelo =
                new ShaperDetalleViewModel
                {
                    Shaper = shaper,
                    Productos = _productoServicio.BuscarPorShaper(id),
                    Disenos = _disenoServicio.ObtenerPorShaper(id, true)
                };

            return View(modelo);
        }
    }
}
