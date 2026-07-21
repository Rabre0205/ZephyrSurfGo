using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Models.Productos
{
    public class TablaViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public string ImagenUrl { get; set; }
        public string ImagenAtrasUrl { get; set; }

        public string Altura { get; set; }
        public int Ancho { get; set; }
        public decimal Volumen { get; set; }
        public string SistemaDeEncaje { get; set; }
        public string TipoDeOla { get; set; }
        public string EstiloDeSurf { get; set; }
        public int PesoMinimo { get; set; }
        public int PesoMaximo { get; set; }
        public string Experiencia { get; set; }
    }
}
