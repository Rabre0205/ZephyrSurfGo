using ClassLibrary.Servicios;
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
            if (!int.TryParse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out int shaperId))
            {
                return Unauthorized();
            }

            string url = _mercadoPagoServicio.ObtenerUrlAutorizacion(shaperId);
            return Redirect(url);
        }

        [Authorize(Roles = "Shaper")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            if (string.IsNullOrWhiteSpace(code) ||
                !int.TryParse(state, out int shaperId))
            {
                TempData["Error"] =
                    "No se pudo validar la respuesta de Mercado Pago.";
                return RedirectToAction("Index", "Dashboard");
            }

            await _mercadoPagoServicio.ProcesarCallbackAsync(code, shaperId);

            TempData["Mensaje"] = "Tu cuenta de MercadoPago quedó conectada.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
