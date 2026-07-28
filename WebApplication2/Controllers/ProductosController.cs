using ClassLibrary.Datos;
using ClassLibrary.Enums;
using ClassLibrary.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models.Productos;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Shaper")]
    public class ProductosController : Controller
    {
        private readonly IProductoRepositorio _productoRepositorio;
        private readonly IWebHostEnvironment _entorno;

        public ProductosController(
            IProductoRepositorio productoRepositorio,
            IWebHostEnvironment entorno)
        {
            _productoRepositorio = productoRepositorio;
            _entorno = entorno;
        }

        public IActionResult Index()
        {
            int shaperId = ObtenerUsuarioId();

            List<Producto> productos =
                _productoRepositorio.ObtenerPorShaper(shaperId);

            return View(productos);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new TablaFormModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            TablaFormModel modelo)
        {
            ValidarFormulario(modelo);

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            string imagenFrontalUrl;
            string imagenTraseraUrl = string.Empty;

            try
            {
                imagenFrontalUrl =
                    await GuardarImagenAsync(
                        modelo.ImagenFrontal
                    );

                if (modelo.ImagenTrasera != null)
                {
                    imagenTraseraUrl =
                        await GuardarImagenAsync(
                            modelo.ImagenTrasera
                        );
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message
                );

                return View(modelo);
            }

            int shaperId = ObtenerUsuarioId();

            Tabla tabla = new Tabla(
                titulo: modelo.Titulo,
                subtitulo: modelo.Subtitulo,
                precio: modelo.Precio,
                descripcion: modelo.Descripcion,
                imagenUrl: imagenFrontalUrl,
                shaperId: shaperId,
                altura: modelo.Altura,
                ancho: modelo.Ancho,
                volumen: modelo.Volumen,
                sis: (SistemaDeEncaje)modelo.SistemaDeEncaje,
                tipodeola: (TipoDeOla)modelo.TipoDeOla,
                estilo: (EstiloDeSurf)modelo.EstiloDeSurf,
                pesomin: modelo.PesoMinimo,
                pesomax: modelo.PesoMaximo,
                exp: (Experiencia)modelo.Experiencia,
                urlimg: imagenTraseraUrl
            );

            try
            {
                _productoRepositorio.InsertarTabla(tabla);
            }
            catch
            {
                EliminarImagenSiExiste(imagenFrontalUrl);
                EliminarImagenSiExiste(imagenTraseraUrl);

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo guardar la tabla."
                );

                return View(modelo);
            }

            TempData["Mensaje"] =
                "Tabla creada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            ViewBag.ProductoId = id;
            return View();
        }

        private int ObtenerUsuarioId()
        {
            string? valor =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!int.TryParse(valor, out int usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "No se pudo identificar al usuario."
                );
            }

            return usuarioId;
        }

        private void ValidarFormulario(
            TablaFormModel modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo.Titulo))
            {
                ModelState.AddModelError(
                    nameof(modelo.Titulo),
                    "El título es obligatorio."
                );
            }

            if (modelo.Precio <= 0)
            {
                ModelState.AddModelError(
                    nameof(modelo.Precio),
                    "El precio debe ser mayor a cero."
                );
            }

            if (modelo.ImagenFrontal == null)
            {
                ModelState.AddModelError(
                    nameof(modelo.ImagenFrontal),
                    "La imagen frontal es obligatoria."
                );
            }

            if (modelo.PesoMinimo < 0)
            {
                ModelState.AddModelError(
                    nameof(modelo.PesoMinimo),
                    "El peso mínimo no puede ser negativo."
                );
            }

            if (modelo.PesoMaximo < modelo.PesoMinimo)
            {
                ModelState.AddModelError(
                    nameof(modelo.PesoMaximo),
                    "El peso máximo debe ser mayor o igual al mínimo."
                );
            }
        }

        private async Task<string> GuardarImagenAsync(
            IFormFile archivo)
        {
            const long tamanioMaximo =
                5 * 1024 * 1024;

            if (archivo.Length <= 0)
            {
                throw new InvalidOperationException(
                    "La imagen está vacía."
                );
            }

            if (archivo.Length > tamanioMaximo)
            {
                throw new InvalidOperationException(
                    "La imagen no puede superar los 5 MB."
                );
            }

            string extension =
                Path.GetExtension(
                    archivo.FileName
                ).ToLowerInvariant();

            string[] extensionesPermitidas =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!extensionesPermitidas.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Solo se permiten imágenes JPG, PNG o WEBP."
                );
            }

            string carpeta =
                Path.Combine(
                    _entorno.WebRootPath,
                    "uploads",
                    "productos"
                );

            Directory.CreateDirectory(carpeta);

            string nombreArchivo =
                $"{Guid.NewGuid()}{extension}";

            string rutaFisica =
                Path.Combine(
                    carpeta,
                    nombreArchivo
                );

            await using FileStream stream =
                new FileStream(
                    rutaFisica,
                    FileMode.Create
                );

            await archivo.CopyToAsync(stream);

            return
                $"/uploads/productos/{nombreArchivo}";
        }

        private void EliminarImagenSiExiste(
            string imagenUrl)
        {
            if (string.IsNullOrWhiteSpace(imagenUrl))
            {
                return;
            }

            string rutaRelativa =
                imagenUrl.TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    );

            string rutaFisica =
                Path.Combine(
                    _entorno.WebRootPath,
                    rutaRelativa
                );

            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }
    }
}