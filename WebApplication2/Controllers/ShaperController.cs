using ClassLibrary.Enums;
using ClassLibrary.Persona;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class ShaperController : Controller
    {
        public IActionResult Detalle(int id)
        {
            Shaper shaper = new Shaper(
                id: id,
                email: "master@surf.com",
                contrasenia: "12345",
                nombre: "Francisco",
                pais: Pais.Uruguay,
                nombreDeNegosio: "Master Surfboards",
                contacto: "099 123 456",
                logoUrl: "/img/logo-master.png"
            );

            ShaperDetalleViewModel modelo =
                new ShaperDetalleViewModel
                {
                    Shaper = shaper,
                    Productos = new List<ClassLibrary.Productos.Producto>()
                };

            return View(modelo);
        }
    }
}