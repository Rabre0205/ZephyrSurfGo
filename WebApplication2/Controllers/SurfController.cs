using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class SurfController : Controller
    {
        public IActionResult carrito() { return View(); }
        public IActionResult Dealers() { return View(); }
        public IActionResult Home() { return View(); }
        public IActionResult login() { return View(); }
        public IActionResult master() { return View(); }
        public IActionResult shapers() { return View(); }
    }
}
