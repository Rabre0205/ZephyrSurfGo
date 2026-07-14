using ClassLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Productos
{
    public class Tabla : Producto
    {
        public string Altura { get; set; }
        public int Ancho { get; set; }
        public double Volumen { get; set; }
        public SistemaDeEncaje SistemaDeEncaje { get; set; }
        public TipoDeOla TipoDeOla { get; set; }
        public EstiloDeSurf EstiloDeSurf { get; set; }
        public int PesoMinimo { get; set; }
        public int PesoMaximo { get; set; }
        public Experiencia Experiencia { get; set; }
        public string ImagenAtrasUrl { get; set; }

        public Tabla(string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId,
            string altura,int ancho, double volumen, SistemaDeEncaje sis, TipoDeOla tipodeola,EstiloDeSurf estilo,int pesomin, int pesomax, Experiencia exp, string urlimg ) 
            :base(titulo, subtitulo, precio, descripcion, imagenUrl, shaperId) {

            Altura = altura;
            Ancho = ancho;
            Volumen = volumen;
            SistemaDeEncaje = sis;
            TipoDeOla = tipodeola;
            EstiloDeSurf = estilo;
            PesoMinimo = pesomin;
            PesoMaximo = pesomax;
            Experiencia = exp;
            ImagenAtrasUrl = urlimg;
        }

        public Tabla(int id, string titulo, string subtitulo, double precio, string descripcion, string imagenUrl, int shaperId,
            string altura, int ancho, double volumen, SistemaDeEncaje sis, TipoDeOla tipodeola, EstiloDeSurf estilo, int pesomin, int pesomax, Experiencia exp, string urlimg)
            : base(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId)
        {
            Altura = altura;
            Ancho = ancho;
            Volumen = volumen;
            SistemaDeEncaje = sis;
            TipoDeOla = tipodeola;
            EstiloDeSurf = estilo;
            PesoMinimo = pesomin;
            PesoMaximo = pesomax;
            Experiencia = exp;
            ImagenAtrasUrl = urlimg;
        }
    }

}
