using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.Controllers
{
    public class MercadoPagoController : Controller
    {
        private readonly IMercadoPagoServicio _mercadoPagoServicio;

        public MercadoPagoController(IMercadoPagoServicio mercadoPagoServicio)
        {
            _mercadoPagoServicio = mercadoPagoServicio;
        }

        [Authorize(Roles = "Shaper")]
        public IActionResult ConectarCuenta()
        {
            int shaperId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string url = _mercadoPagoServicio.ObtenerUrlAutorizacion(shaperId);
            return Redirect(url);
        }

        [Authorize(Roles = "Shaper")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            int shaperId = int.Parse(state);
            await _mercadoPagoServicio.ProcesarCallbackAsync(code, shaperId);

            TempData["Mensaje"] = "Tu cuenta de MercadoPago quedó conectada.";
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
