using ClassLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Productos
{
    public class Traje : Producto
    {
        public Genero Genero { get; set; }
        public int Espesor{ get; set; }
        public Talle Talle { get; set; }
        public string Temperatura { get; set; }

        public Traje(string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, Genero genero, int espesor, Talle talle,string temperatura)
    : base(titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            Genero = genero;
            Espesor = espesor;
            Talle = talle;
            Temperatura = temperatura;
        }

        public Traje(int id, string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId, Genero genero, int espesor, Talle talle, string temperatura)
            : base(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            Genero = genero;
            Espesor = espesor;
            Talle = talle;
            Temperatura = temperatura;
        }

    }
}
