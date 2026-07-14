using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Productos
{
    public class Leash : Producto
    {
        public int LargoDeTablaRecomendado { get; set; }
        
        public Leash(string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, int largoDeTablaRecomendado)
            : base(titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            LargoDeTablaRecomendado = largoDeTablaRecomendado;
        }

        public Leash(int id, string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, int largoDeTablaRecomendado)
            : base(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            LargoDeTablaRecomendado = largoDeTablaRecomendado;
        }

    }
}
