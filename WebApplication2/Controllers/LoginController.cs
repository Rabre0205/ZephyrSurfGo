using Microsoft.AspNetCore.Mvc;
using ClassLibrary;
using ClassLibrary.Persona;

namespace WebApplication2.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IniciarSesion(string email, string password)
        {

            Sistema sistema = Sistema.ObtenerInstancia();

            Usuario u = sistema.Login(email, password);

            if (u != null)
            {
                HttpContext.Session.SetString("Rol", u.TipoDeUsuario.ToString());
                HttpContext.Session.SetString("UId", u.Id.ToString());

                return RedirectToAction("Home", "Surf");
            }
            else
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrarse(
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
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorRegistro =
                    "Completá todos los campos obligatorios.";

                return View("Index");
            }

            if (password.Length < 8)
            {
                ViewBag.ErrorRegistro =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View("Index");
            }

            if (password != confirmarPassword)
            {
                ViewBag.ErrorRegistro =
                    "Las contraseñas no coinciden.";

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
                out ClassLibrary.Enums.Pais paisSeleccionado))
            {
                ViewBag.ErrorRegistro =
                    "Seleccioná un país válido.";

                return View("Index");
            }

            try
            {
                Sistema sistema = Sistema.ObtenerInstancia();

                Usuario usuario = sistema.RegistrarCliente(
                    email,
                    $"{nombre.Trim()} {apellido.Trim()}",
                    paisSeleccionado,
                    password
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

                return RedirectToAction("Home", "Surf");
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.ErrorRegistro = ex.Message;
                ViewBag.MostrarRegistro = true;
                return View("Index");
            }
            catch (Exception)
            {
                ViewBag.ErrorRegistro = "Ocurrió un error al crear la cuenta.";
                ViewBag.MostrarRegistro = true;
                return View("Index");
            }
        }

        public IActionResult Logout()
        {
            Sistema sistema = Sistema.ObtenerInstancia();
            HttpContext.Session.Clear();

            return RedirectToAction("Home", "Surf");
        }
    }
}