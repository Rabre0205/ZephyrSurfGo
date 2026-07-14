using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Productos
{
    public class Pad : Producto
    {
        public int Largo { get; set; }
        public int Ancho { get; set; }
        public string Material { get; set; }

        public Pad(string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, int largo, int ancho, string material)
            : base(titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            Largo = largo;
            Ancho = ancho;
            Material = material;
        }

        public Pad(int id, string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, int largo, int ancho, string material)
            : base(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            Largo = largo;
            Ancho = ancho;
            Material = material;
        }

    }
}
