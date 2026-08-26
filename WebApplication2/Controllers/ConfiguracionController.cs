using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication2.Models.Configuracion;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Cliente,Shaper")]
    public class ConfiguracionController : Controller
    {
        private readonly IUsuarioServicio _usuarioServicio;
        private readonly IWebHostEnvironment _entorno;

        public ConfiguracionController(IUsuarioServicio usuarioServicio, IWebHostEnvironment entorno)
        {
            _usuarioServicio = usuarioServicio;
            _entorno = entorno;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int usuarioId = ObtenerUsuarioId();
            var usuario = _usuarioServicio.BuscarPorId(usuarioId);
            if (usuario == null) return NotFound();

            return View(new ConfiguracionViewModel
            {
                EsShaper = usuario is ClassLibrary.Persona.Shaper,
                LogoUrl = (usuario as ClassLibrary.Persona.Shaper)?.LogoUrl,
                Cuenta = new ConfiguracionCuentaViewModel
                {
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Pais = usuario.Pais
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(2_200_000)]
        public async Task<IActionResult> ActualizarLogo(IFormFile? logo, bool eliminarLogo = false)
        {
            int usuarioId = ObtenerUsuarioId();
            var shaper = _usuarioServicio.BuscarPorId(usuarioId) as ClassLibrary.Persona.Shaper;
            if (shaper == null) return Forbid();

            if (!eliminarLogo && (logo == null || logo.Length == 0))
            {
                TempData["ErrorLogo"] = "Seleccioná una imagen para continuar.";
                return RedirectToAction(nameof(Index));
            }

            string? nuevaUrl = null;
            string? nuevaRuta = null;
            if (!eliminarLogo)
            {
                if (logo!.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorLogo"] = "El logo no puede superar los 2 MB.";
                    return RedirectToAction(nameof(Index));
                }

                string? extension = await ObtenerExtensionSegura(logo);
                if (extension == null)
                {
                    TempData["ErrorLogo"] = "Usá una imagen JPG, PNG o WebP válida.";
                    return RedirectToAction(nameof(Index));
                }

                string carpeta = Path.Combine(_entorno.WebRootPath, "uploads", "perfiles");
                Directory.CreateDirectory(carpeta);
                string nombre = $"shaper-{usuarioId}-{Guid.NewGuid():N}{extension}";
                nuevaRuta = Path.Combine(carpeta, nombre);
                await using var destino = new FileStream(nuevaRuta, FileMode.CreateNew);
                await logo.CopyToAsync(destino);
                nuevaUrl = $"/uploads/perfiles/{nombre}";
            }

            if (!_usuarioServicio.ActualizarLogoShaper(usuarioId, nuevaUrl))
            {
                BorrarArchivoLocal(nuevaUrl);
                TempData["ErrorLogo"] = "No se pudo actualizar el logo de la marca.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.Equals(shaper.LogoUrl, nuevaUrl, StringComparison.OrdinalIgnoreCase))
                BorrarArchivoLocal(shaper.LogoUrl);
            await RenovarSesion(usuarioId);
            TempData["MensajeLogo"] = eliminarLogo ? "El logo fue eliminado." : "El logo de la marca se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCuenta(
            [Bind(Prefix = "Cuenta")] ConfiguracionCuentaViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View("Index", new ConfiguracionViewModel { Cuenta = modelo });

            int usuarioId = ObtenerUsuarioId();
            var resultado = _usuarioServicio.ActualizarCuenta(
                usuarioId, modelo.Email, modelo.Nombre, modelo.Pais);

            if (!resultado.Exito)
            {
                ViewData["ErrorCuenta"] = resultado.Error;
                return View("Index", new ConfiguracionViewModel { Cuenta = modelo });
            }

            await RenovarSesion(usuarioId);
            TempData["MensajeCuenta"] = "La información de tu cuenta se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarContrasenia(
            [Bind(Prefix = "Seguridad")] CambiarContraseniaViewModel modelo)
        {
            ModelState.Clear();
            if (!TryValidateModel(modelo, "Seguridad"))
                return VistaConCuenta(new ConfiguracionViewModel { Seguridad = modelo });

            var resultado = _usuarioServicio.CambiarContrasenia(
                ObtenerUsuarioId(), modelo.ContraseniaActual,
                modelo.NuevaContrasenia, modelo.ConfirmarContrasenia);

            if (!resultado.Exito)
            {
                ViewData["ErrorSeguridad"] = resultado.Error;
                return VistaConCuenta(new ConfiguracionViewModel { Seguridad = modelo });
            }

            TempData["MensajeSeguridad"] = "Tu contraseña se cambió correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult VistaConCuenta(ConfiguracionViewModel modelo)
        {
            var usuario = _usuarioServicio.BuscarPorId(ObtenerUsuarioId());
            if (usuario == null) return NotFound();

            modelo.Cuenta = new ConfiguracionCuentaViewModel
            {
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Pais = usuario.Pais
            };
            modelo.EsShaper = usuario is ClassLibrary.Persona.Shaper;
            modelo.LogoUrl = (usuario as ClassLibrary.Persona.Shaper)?.LogoUrl;
            return View("Index", modelo);
        }

        private int ObtenerUsuarioId()
        {
            string? valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(valor, out int usuarioId))
                throw new InvalidOperationException("No se pudo identificar al usuario autenticado.");
            return usuarioId;
        }

        private async Task RenovarSesion(int usuarioId)
        {
            var usuario = _usuarioServicio.BuscarPorId(usuarioId);
            if (usuario == null)
            {
                throw new InvalidOperationException(
                    "No se pudo renovar la sesión del usuario.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Role, usuario.TipoDeUsuario.ToString()),
                new("metodo_autenticacion",
                    User.FindFirstValue("metodo_autenticacion") ?? "Password")
            };
            if (usuario is ClassLibrary.Persona.Shaper shaper && !string.IsNullOrWhiteSpace(shaper.LogoUrl))
                claims.Add(new Claim("logo_url", shaper.LogoUrl));
            var identidad = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));
        }

        private static async Task<string?> ObtenerExtensionSegura(IFormFile archivo)
        {
            byte[] cabecera = new byte[12];
            await using var stream = archivo.OpenReadStream();
            int leidos = await stream.ReadAsync(cabecera.AsMemory(0, cabecera.Length));
            if (leidos >= 8 && cabecera.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return ".png";
            if (leidos >= 3 && cabecera[0] == 255 && cabecera[1] == 216 && cabecera[2] == 255) return ".jpg";
            if (leidos >= 12 && System.Text.Encoding.ASCII.GetString(cabecera, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(cabecera, 8, 4) == "WEBP") return ".webp";
            return null;
        }

        private void BorrarArchivoLocal(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/perfiles/", StringComparison.OrdinalIgnoreCase)) return;
            string carpeta = Path.GetFullPath(Path.Combine(_entorno.WebRootPath, "uploads", "perfiles"));
            string ruta = Path.GetFullPath(Path.Combine(_entorno.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
            if (Path.GetDirectoryName(ruta)?.Equals(carpeta, StringComparison.OrdinalIgnoreCase) == true && System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);
        }
    }
}
