using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PanelAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }