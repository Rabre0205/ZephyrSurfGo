using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace WebApplication2.Models.Productos
{
    /// <summary>
    /// Modelo DTO para recibir datos del formulario de creación de Tabla.
    /// Se usa en controllers/razor pages para mapear datos del formulario HTML.
    /// </summary>
    public class TablaFormModel
    {
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public double Precio { get; set; }
        public string Descripcion { get; set; }

        // Archivos de imagen
        public IFormFile ImagenFrontal { get; set; }  // Imagen principal
        public IFormFile ImagenTrasera { get; set; }  // Imagen trasera (opcional)

        // Especificaciones de la tabla
        public string Altura { get; set; }
        public int Ancho { get; set; }
        public double Volumen { get; set; }
        public byte SistemaDeEncaje { get; set; }    // 0=FSS2, 1=Future
        public byte TipoDeOla { get; set; }          // 0=Plana, 1=Power, 2=Chica
        public byte EstiloDeSurf { get; set; }       // 0=Agresivo, 1=Fluido, 2=Versatil, 3=Recreativo
        public int PesoMinimo { get; set; }
        public int PesoMaximo { get; set; }
        public byte Experiencia { get; set; }        // 0=SinExp, 1=Iniciado, 2=Intermedio, 3=Avanzado
    }
}
