using ClassLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Productos
{
    public class Quilla : Producto
    {
        public SistemaDeEncaje SistemaDeEncaje { get; set; }

        public Quilla(string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, SistemaDeEncaje sistemaDeEncaje)
            : base(titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            SistemaDeEncaje = sistemaDeEncaje;
        }

        public Quilla(int id, string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, SistemaDeEncaje sistemaDeEncaje)
            : base(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            SistemaDeEncaje = sistemaDeEncaje;
        }
    }
}
