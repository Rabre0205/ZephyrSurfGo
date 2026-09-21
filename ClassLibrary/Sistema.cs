using ClassLibrary.Datos;
using ClassLibrary.Enums;
using ClassLibrary.Persona;
using ClassLibrary.Productos;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassLibrary
{
    public class Sistema
    {

        private static Sistema? _instancia;

        private readonly UsuarioRepositorio usuarioRepositorio;

        public List<Usuario> Usuarios { get; private set; }

        /// <summary>
        /// Singleton: obtiene o crea la instancia única del sistema.
        /// </summary>
        public static Sistema ObtenerInstancia()
        {
            if (_instancia == null)
            {
                _instancia = new Sistema();
            }
            return _instancia;
        }

        private Sistema()
        {
            usuarioRepositorio = new UsuarioRepositorio();

            Usuarios = new List<Usuario>();
            CargarDatos();
        }



        public void CargarDatos()
        {
            Usuarios = usuarioRepositorio.ObtenerTodos();
        }

  
        
        public Usuario? Login(string email, string contrasenia)
        {
            foreach (Usuario usuario in Usuarios)
            {
                bool mismoEmail = string.Equals(
                    usuario.Email?.Trim(),
                    email?.Trim(),
                    StringComparison.OrdinalIgnoreCase
                );

                bool mismaContrasenia =
                    usuario.Contrasenia == contrasenia;

                if (mismoEmail && mismaContrasenia)
                {
                    return usuario;
                }
            }

            return null;
        }
    }
}

