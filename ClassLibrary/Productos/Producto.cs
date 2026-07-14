using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Productos
{
    public class Producto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public double Precio { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public int ShaperId { get; set; }

        public Producto(int id,string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId)
        {
            Id = id;
            Titulo = titulo;
            Subtitulo = subtitulo;
            Precio = precio;
            Descripcion = descripcion;
            ImagenUrl = imagenUrl;
            ShaperId = shaperId;
        }

        public Producto(string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId)
        {
            Titulo = titulo;
            Subtitulo = subtitulo;
            Precio = precio;
            Descripcion = descripcion;
            ImagenUrl = imagenUrl;
            ShaperId = shaperId;
        }



    }
}
