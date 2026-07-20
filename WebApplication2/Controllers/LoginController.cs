using Microsoft.AspNetCore.Mvc;

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
            if (email == "cliente@gmail.com" && password == "1234")
            {
                HttpContext.Session.SetString("Rol", "Cliente");
                HttpContext.Session.SetString("Usuario", "Cliente");

                return RedirectToAction("Home", "Surf");
            }

            if (email == "shaper@gmail.com" && password == "1234")
            {
                HttpContext.Session.SetString("Rol", "Shaper");
                HttpContext.Session.SetString("Usuario", "Shaper");

                return RedirectToAction("Home", "Surf");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Home", "Surf");
        }
    }
}