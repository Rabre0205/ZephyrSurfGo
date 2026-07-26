using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ClassLibrary.Enums;
using ClassLibrary.Persona;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Mvc;

public class LoginController : Controller
{
    private readonly IUsuarioServicio _usuarioServicio;

    public LoginController(IUsuarioServicio usuarioServicio)
    {
        _usuarioServicio = usuarioServicio;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(
        string email,
        string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error =
                "Ingresá el correo y la contraseña.";

            return View("Index");
        }

        try
        {
            Usuario usuario = _usuarioServicio.Login(
                email.Trim(),
                password
            );

            if (usuario == null)
            {
                ViewBag.Error =
                    "Usuario o contraseña incorrectos.";

                return View("Index");
            }

            await GuardarUsuarioEnSesion(usuario);

            return RedirectToAction("Home", "Surf");
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.ToString();
            return View("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrarse(
        string nombre,
        string apellido,
        string email,
        string pais,
        string password,
        string confirmarPassword,
        bool aceptaTerminos)
    {
        ViewBag.MostrarRegistro = true;

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(pais) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmarPassword))
        {
            ViewBag.ErrorRegistro =
                "Completá todos los campos obligatorios.";

            return View("Index");
        }

        if (!aceptaTerminos)
        {
            ViewBag.ErrorRegistro =
                "Debés aceptar los términos y condiciones.";

            return View("Index");
        }

        if (!Enum.TryParse(
                pais,
                true,
                out Pais paisSeleccionado))
        {
            ViewBag.ErrorRegistro =
                "Seleccioná un país válido.";

            return View("Index");
        }

        try
        {
            var resultado = _usuarioServicio.RegistrarCliente(
                email.Trim(),
                $"{nombre.Trim()} {apellido.Trim()}",
                paisSeleccionado,
                password,
                confirmarPassword
            );

            if (!resultado.Exito)
            {
                ViewBag.ErrorRegistro = resultado.Error;
                ViewBag.MostrarRegistro = true;

                return View("Index");
            }

            Usuario usuario = _usuarioServicio.BuscarPorId(
                resultado.UsuarioId
            );

            if (usuario == null)
            {
                ViewBag.ErrorRegistro =
                    "El usuario fue creado, pero no se pudo iniciar sesión.";

                ViewBag.MostrarRegistro = true;

                return View("Index");
            }

            await GuardarUsuarioEnSesion(usuario);

            return RedirectToAction("Home", "Surf");
        }
        catch (Exception)
        {
            ViewBag.ErrorRegistro =
                "Ocurrió un error al crear la cuenta.";

            ViewBag.MostrarRegistro = true;

            return View("Index");
        }
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        HttpContext.Session.Clear();

        return RedirectToAction("Home", "Surf");
    }

    private async Task GuardarUsuarioEnSesion(
        Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                usuario.Id.ToString()
            ),
            new Claim(
                ClaimTypes.Name,
                usuario.Nombre
            ),
            new Claim(
                ClaimTypes.Role,
                usuario.TipoDeUsuario.ToString()
            )
        };

        var identidad = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identidad);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        HttpContext.Session.SetString(
            "Rol",
            usuario.TipoDeUsuario.ToString()
        );

        HttpContext.Session.SetString(
            "UId",
            usuario.Id.ToString()
        );

        HttpContext.Session.SetString(
            "Usuario",
            usuario.Nombre
        );
    }
}