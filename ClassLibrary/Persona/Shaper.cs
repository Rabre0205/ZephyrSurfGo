using ClassLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClassLibrary.Persona
{
    public class Shaper : Usuario
    {
        public string NombreDeNegosio { get; set; }
        public string Contacto { get; set; }
        public string LogoUrl { get; set; }

        public Shaper(int id, string email, string contrasenia, string nombre, Pais pais, string nombreDeNegosio, string contacto, string logoUrl)
        : base(id, email, nombre, pais, contrasenia)
        {
            NombreDeNegosio = nombreDeNegosio;
            Contacto = contacto;
            LogoUrl = logoUrl;
        }
        public Shaper( string email, string contrasenia, string nombre, Pais pais, string nombreDeNegosio, string contacto, string logoUrl)
        : base(email, nombre, pais, contrasenia)
        {
            NombreDeNegosio = nombreDeNegosio;
            Contacto = contacto;
            LogoUrl = logoUrl;
        }
    }
}
