using ClassLibrary.Datos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication2.ViewComponents
{
    public class CarritoCantidadViewComponent : ViewComponent
    {
        private readonly ICarritoRepositorio _carritoRepositorio;

        public CarritoCantidadViewComponent(ICarritoRepositorio carritoRepositorio)
        {
            _carritoRepositorio = carritoRepositorio;
        }

        public IViewComponentResult Invoke()
        {
            if (UserClaimsPrincipal.Identity?.IsAuthenticated != true ||
                !UserClaimsPrincipal.IsInRole("Cliente"))
            {
                return Content("0");
            }

            string? idClaim = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out int clienteId))
            {
                return Content("0");
            }

            int cantidad = _carritoRepositorio.ObtenerPorUsuario(clienteId).Sum(item => item.Cantidad);
            return Content(cantidad.ToString());
        }
    }
}
