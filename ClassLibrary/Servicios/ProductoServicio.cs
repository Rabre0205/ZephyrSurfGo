using ClassLibrary.Datos;
using ClassLibrary.Enums;
using ClassLibrary.Productos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Servicios
{
    public interface IProductoServicio
    {
        List<Producto> BuscarPorShaper(int shaperId);

        List<Tabla> ObtenerTablasDelShaper(int shaperId);

        int ContarProductosPublicados();

        int AgregarTabla(
            string titulo,
            string subtitulo,
            double precio,
            string descripcion,
            int shaperId,
            string altura,
            int ancho,
            double volumen,
            SistemaDeEncaje sistemaDeEncaje,
            TipoDeOla tipoDeOla,
            EstiloDeSurf estiloDeSurf,
            int pesoMinimo,
            int pesoMaximo,
            Experiencia experiencia,
            IFormFile imagenFrontal,
            IFormFile imagenTrasera
        );
    }

    public class ProductoServicio : IProductoServicio
    {
        private readonly IProductoRepositorio _productoRepositorio;
        private readonly ICloudinaryServicio _cloudinarioServicio;

        public ProductoServicio(
            IProductoRepositorio productoRepositorio,
            ICloudinaryServicio cloudinarioServicio)
        {
            _productoRepositorio = productoRepositorio;
            _cloudinarioServicio = cloudinarioServicio;
        }

        public List<Producto> BuscarPorShaper(int shaperId)
        {
            return _productoRepositorio.ObtenerPorShaper(shaperId);
        }

        public int ContarProductosPublicados()
        {
            return _productoRepositorio.ContarProductosPublicados();
        }

        public List<Tabla> ObtenerTablasDelShaper(int shaperId)
        {
            List<Tabla> tablas = new List<Tabla>();

            foreach (Producto producto in BuscarPorShaper(shaperId))
            {
                if (producto is Tabla tabla)
                {
                    tablas.Add(tabla);
                }
            }

            return tablas;
        }

        public int AgregarTabla(
            string titulo,
            string subtitulo,
            double precio,
            string descripcion,
            int shaperId,
            string altura,
            int ancho,
            double volumen,
            SistemaDeEncaje sistemaDeEncaje,
            TipoDeOla tipoDeOla,
            EstiloDeSurf estiloDeSurf,
            int pesoMinimo,
            int pesoMaximo,
            Experiencia experiencia,
            IFormFile imagenFrontal,
            IFormFile imagenTrasera)
        {
            if (imagenFrontal == null || imagenFrontal.Length == 0)
            {
                throw new ArgumentException(
                    "La imagen frontal es requerida."
                );
            }

            string timestamp =
                DateTime.Now.Ticks.ToString();

            string nombreLimpio =
                titulo
                    .Replace(" ", "_")
                    .Replace("'", "");

            string urlFrontal =
                _cloudinarioServicio.SubirImagen(
                    imagenFrontal,
                    $"tabla_{nombreLimpio}_{timestamp}_frontal"
                );

            string urlTrasera = "";

            if (imagenTrasera != null &&
                imagenTrasera.Length > 0)
            {
                urlTrasera =
                    _cloudinarioServicio.SubirImagen(
                        imagenTrasera,
                        $"tabla_{nombreLimpio}_{timestamp}_trasera"
                    ) ?? "";
            }

            var tabla = new Tabla(
                titulo,
                subtitulo,
                precio,
                descripcion,
                urlFrontal,
                shaperId,
                altura,
                ancho,
                volumen,
                sistemaDeEncaje,
                tipoDeOla,
                estiloDeSurf,
                pesoMinimo,
                pesoMaximo,
                experiencia,
                urlTrasera
            );

            return _productoRepositorio.InsertarTabla(tabla);
        }
    }
}