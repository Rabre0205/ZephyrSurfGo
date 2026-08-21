using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        // ensure the root URL (/) is handled by this action even if a static index exists
        //[HttpGet("/")]
        public IActionResult Index()
        {
            return Redirect("/Surf/Home");
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            int codigo = statusCode ?? StatusCodes.Status500InternalServerError;
            Response.StatusCode = codigo;

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = codigo
            });
        }

    }
}
