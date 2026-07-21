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

            if (sistema.Login(email, password) != null)
            {
                Usuario u = sistema.Login(email, password);

                HttpContext.Session.SetString("Rol", u.TipoDeUsuario.ToString());
                HttpContext.Session.SetString("UId", u.Id.ToString());
                return View("Surf/Home");
            }
            else {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
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