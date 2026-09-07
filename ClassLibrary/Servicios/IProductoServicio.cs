using ClassLibrary.Productos;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ClassLibrary.Servicios
{
    public interface IProductoServicio
    {
        List<Producto> BuscarPorShaper(int shaperId);
        List<Tabla> ObtenerTablasDelShaper(int shaperId);
        int AgregarTabla(
            string titulo, string subtitulo, double precio, string descripcion, int shaperId,
            string altura, int ancho, double volumen,
            ClassLibrary.Enums.SistemaDeEncaje sistemaDeEncaje, ClassLibrary.Enums.TipoDeOla tipoDeOla, ClassLibrary.Enums.EstiloDeSurf estiloDeSurf,
            int pesoMinimo, int pesoMaximo, ClassLibrary.Enums.Experiencia experiencia,
            IFormFile imagenFrontal, IFormFile imagenTrasera);
    }
}
