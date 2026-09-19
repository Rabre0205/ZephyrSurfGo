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
        private readonly ClassLibrary.Datos.IInteraccionesRepositorio _interacciones;
        private readonly ClassLibrary.Datos.ICarritoRepositorio _carritoRepositorio;
        private readonly ClassLibrary.Datos.ICursoRepositorio _cursoRepositorio;

        public ShaperController(
            ClassLibrary.Servicios.IUsuarioServicio usuarioServicio,
            ClassLibrary.Servicios.IProductoServicio productoServicio,
            ClassLibrary.Servicios.IDisenoShaperServicio disenoServicio,
            ClassLibrary.Datos.IInteraccionesRepositorio interacciones,
            ClassLibrary.Datos.ICarritoRepositorio carritoRepositorio,
            ClassLibrary.Datos.ICursoRepositorio cursoRepositorio)
        {
            _usuarioServicio = usuarioServicio;
            _productoServicio = productoServicio;
            _disenoServicio = disenoServicio;
            _interacciones = interacciones;
            _carritoRepositorio = carritoRepositorio;
            _cursoRepositorio = cursoRepositorio;
        }

        public IActionResult Detalle(int id, int? disenoGuardado = null)
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

            var productos = _productoServicio.BuscarPorShaper(id);
            int clienteId = User.IsInRole("Cliente") && int.TryParse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int valorCliente)
                ? valorCliente : 0;

            ShaperDetalleViewModel modelo =
                new ShaperDetalleViewModel
                {
                    Shaper = shaper,
                    Productos = productos,
                    Disenos = _disenoServicio.ObtenerPorShaper(id, true),
                    Resenas = _interacciones.ObtenerResumenes(productos.Select(p => p.Id)),
                    Favoritos = clienteId > 0
                        ? productos.Where(p => _interacciones.EsFavorito(clienteId, p.Id)).Select(p => p.Id).ToHashSet()
                        : new HashSet<int>(),
                    ConfiguracionGuardadaJson = clienteId > 0 && disenoGuardado.HasValue
                        ? _interacciones.ObtenerDiseno(disenoGuardado.Value, clienteId)?.ConfiguracionJson
                        : null,
                    Cursos = _cursoRepositorio.ObtenerPorShaper(id, true)
                };

            return View(modelo);
        }

        public IActionResult Producto(int id)
        {
            if (User.Identity?.IsAuthenticated != true) return Challenge();
            var producto = _productoServicio.ObtenerProducto(id);
            if (producto == null) return NotFound();
            if (User.IsInRole("Shaper"))
            {
                string? valor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(valor, out int shaperId) || shaperId != producto.ShaperId) return Forbid();
            }
            else if (!User.IsInRole("Cliente")) return Forbid();

            var shaper = _usuarioServicio.ObtenerShaperPorId(producto.ShaperId);
            if (shaper == null || !shaper.Activo) return NotFound();
            int clienteId = User.IsInRole("Cliente") && int.TryParse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int idCliente) ? idCliente : 0;
            var disponibilidad = _carritoRepositorio.ObtenerDisponibilidad(id);
            var relacionados = _productoServicio.BuscarPorShaper(producto.ShaperId)
                .Where(item => item.Id != producto.Id)
                .Take(3)
                .ToList();
            return View("Producto", new ProductoDetalleViewModel
            {
                Producto=producto, Shaper=shaper,
                Resenas=_interacciones.ObtenerResenas(id),
                EsFavorito=clienteId>0 && _interacciones.EsFavorito(clienteId,id),
                Disponible=disponibilidad?.Disponible == true,
                Stock=disponibilidad?.Stock,
                EsPropietario=User.IsInRole("Shaper"),
                Relacionados=relacionados
            });
        }
    }
}
