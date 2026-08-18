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

        public ConfiguracionController(IUsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int usuarioId = ObtenerUsuarioId();
            var usuario = _usuarioServicio.BuscarPorId(usuarioId);
            if (usuario == null) return NotFound();

            return View(new ConfiguracionViewModel
            {
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
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Role, usuario.TipoDeUsuario.ToString())
            };
            var identidad = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));
        }
    }
}
