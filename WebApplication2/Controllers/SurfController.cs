using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class SurfController : Controller
    {
        public IActionResult carrito() { return View(); }
        public IActionResult Dealers() { return View(); }
        public IActionResult Home()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction(
                        "Index",
                        "PanelAdmin"
                    );
                }

                if (User.IsInRole("Shaper"))
                {
                    return RedirectToAction(
                        "Index",
                        "Dashboard"
                    );
                }
            }

            return View();
        }
        public IActionResult master() { return View(); }
        public IActionResult shapers() { return View(); }
    }
}
