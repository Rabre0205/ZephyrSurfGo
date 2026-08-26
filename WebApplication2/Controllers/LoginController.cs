using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Cryptography;
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
   

    [HttpGet("/Login")]
    [HttpGet("/Login/Index")]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AccesoDenegado()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult IniciarConGoogle()
    {
        var propiedades = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback))
        };

        return Challenge(propiedades, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
        var resultado = await HttpContext.AuthenticateAsync("GoogleTemporal");

        if (!resultado.Succeeded || resultado.Principal == null)
        {
            TempData["ErrorGoogle"] = "No se pudo completar el acceso con Google.";
            return RedirectToAction(nameof(Index));
        }

        string? email = resultado.Principal.FindFirstValue(ClaimTypes.Email);
        string? nombre = resultado.Principal.FindFirstValue(ClaimTypes.Name);
        string? emailVerificado = resultado.Principal.FindFirstValue("google_email_verified");

        if (string.IsNullOrWhiteSpace(email) ||
            !string.Equals(emailVerificado, "true", StringComparison.OrdinalIgnoreCase))
        {
            await HttpContext.SignOutAsync("GoogleTemporal");
            TempData["ErrorGoogle"] = "Google no proporcionó un correo verificado.";
            return RedirectToAction(nameof(Index));
        }

        Usuario? usuario = _usuarioServicio.BuscarPorEmail(email);

        if (usuario != null)
        {
            await HttpContext.SignOutAsync("GoogleTemporal");

            if (!usuario.Activo)
            {
                TempData["ErrorGoogle"] = "Esta cuenta está desactivada.";
                return RedirectToAction(nameof(Index));
            }

            await GuardarUsuarioEnSesion(usuario, "Google");
            return RedirigirSegunRol(usuario);
        }

        HttpContext.Session.SetString("GoogleRegistroEmail", email.Trim());
        HttpContext.Session.SetString(
            "GoogleRegistroNombre",
            string.IsNullOrWhiteSpace(nombre) ? email.Split('@')[0] : nombre.Trim());

        await HttpContext.SignOutAsync("GoogleTemporal");
        return RedirectToAction(nameof(CompletarRegistroGoogle));
    }

    [HttpGet]
    public IActionResult CompletarRegistroGoogle()
    {
        string? email = HttpContext.Session.GetString("GoogleRegistroEmail");
        string? nombre = HttpContext.Session.GetString("GoogleRegistroNombre");

        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.EmailGoogle = email;
        ViewBag.NombreGoogle = nombre;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletarRegistroGoogle(string pais, bool aceptaTerminos)
    {
        string? email = HttpContext.Session.GetString("GoogleRegistroEmail");
        string? nombre = HttpContext.Session.GetString("GoogleRegistroNombre");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nombre))
        {
            TempData["ErrorGoogle"] = "La solicitud de Google venció. Intentá nuevamente.";
            return RedirectToAction(nameof(Index));
        }

        if (!aceptaTerminos || !Enum.TryParse(pais, true, out Pais paisSeleccionado))
        {
            ViewBag.EmailGoogle = email;
            ViewBag.NombreGoogle = nombre;
            ViewBag.ErrorGoogle = !aceptaTerminos
                ? "Debés aceptar los términos y condiciones."
                : "Seleccioná un país válido.";
            return View();
        }

        string claveInterna = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var registro = _usuarioServicio.RegistrarCliente(
            email, nombre, paisSeleccionado, claveInterna, claveInterna);

        if (!registro.Exito)
        {
            ViewBag.EmailGoogle = email;
            ViewBag.NombreGoogle = nombre;
            ViewBag.ErrorGoogle = registro.Error;
            return View();
        }

        Usuario? usuario = _usuarioServicio.BuscarPorId(registro.UsuarioId);
        if (usuario == null)
        {
            TempData["ErrorGoogle"] = "La cuenta fue creada, pero no se pudo iniciar sesión.";
            return RedirectToAction(nameof(Index));
        }

        HttpContext.Session.Remove("GoogleRegistroEmail");
        HttpContext.Session.Remove("GoogleRegistroNombre");
        await GuardarUsuarioEnSesion(usuario, "Google");
        return RedirectToAction("Home", "Surf");
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
            Usuario? usuario = _usuarioServicio.Login(
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

            if (usuario.TipoDeUsuario == TipoDeUsuario.Administrador)
            {
                return RedirectToAction("Index", "PanelAdmin");
            }

            if (usuario.TipoDeUsuario == TipoDeUsuario.Shaper)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return RedirectToAction("Home", "Surf");
        }
        catch (Exception)
        {
            ViewBag.Error =
                "Ocurrió un error al iniciar sesión. Intentá nuevamente.";
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

            Usuario? usuario = _usuarioServicio.BuscarPorId(
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Login");
    }



    private async Task GuardarUsuarioEnSesion(
        Usuario usuario,
        string metodoAutenticacion = "Password")
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
            ),
            new Claim("metodo_autenticacion", metodoAutenticacion)
        };
        if (usuario is Shaper shaper && !string.IsNullOrWhiteSpace(shaper.LogoUrl))
            claims.Add(new Claim("logo_url", shaper.LogoUrl));

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

    private IActionResult RedirigirSegunRol(Usuario usuario)
    {
        if (usuario.TipoDeUsuario == TipoDeUsuario.Administrador)
        {
            return RedirectToAction("Index", "PanelAdmin");
        }

        if (usuario.TipoDeUsuario == TipoDeUsuario.Shaper)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Home", "Surf");
    }
}
