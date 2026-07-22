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
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingresá el correo y la contraseña.";
                return View("Index");
            }

            Sistema sistema = Sistema.ObtenerInstancia();

            Usuario? usuario = sistema.Login(
                email.Trim(),
                password
            );

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View("Index");
            }

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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Home", "Surf");
        }
    }
}