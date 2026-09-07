using ClassLibrary.Datos;
using ClassLibrary.Enums;
using ClassLibrary.Persona;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;


namespace ClassLibrary.Servicios
{
    public class UsuarioServicio : IUsuarioServicio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ICloudinaryServicio _cloudinarioServicio; 


        public UsuarioServicio(IUsuarioRepositorio usuarioRepositorio, ICloudinaryServicio cloudinarioServicio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _cloudinarioServicio = cloudinarioServicio;
        }

        public Usuario Login(string email, string contrasenia)
        {
            Usuario usuario = _usuarioRepositorio.ObtenerPorEmail(email);
            if (usuario == null) return null;

            bool esValida = BCrypt.Net.BCrypt.Verify(contrasenia, usuario.Contrasenia);
            return esValida ? usuario : null;
        }

        public (bool Exito, string Error, int UsuarioId) RegistrarCliente(
        string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasenia))
                return (false, "Completá todos los campos.", 0);

            if (contrasenia.Length < 6)
                return (false, "La contraseña debe tener al menos 6 caracteres.", 0);

            if (contrasenia != confirmarContrasenia)
                return (false, "Las contraseñas no coinciden.", 0);

            Usuario existente = _usuarioRepositorio.ObtenerPorEmail(email.Trim());
            if (existente != null)
                return (false, "Ya existe un usuario registrado con ese email.", 0);

            string hash = BCrypt.Net.BCrypt.HashPassword(contrasenia);

            Usuario nuevoUsuario = new Usuario(email.Trim(), nombre, pais, hash)
            {
                TipoDeUsuario = TipoDeUsuario.Cliente
            };

            int idGenerado = _usuarioRepositorio.InsertarUsuario(nuevoUsuario);

            if (idGenerado <= 0)
                return (false, "Ocurrió un error al registrar el cliente. Intentá nuevamente.", 0);

            return (true, null, idGenerado);
        }

        public (bool Exito, string Error, int UsuarioId) RegistrarShaper(
            string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia,
            string nombreDeNegosio, string contacto)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasenia))
                return (false, "Completá todos los campos.", 0);

            if (string.IsNullOrWhiteSpace(nombreDeNegosio) || string.IsNullOrWhiteSpace(contacto))
                return (false, "El nombre de negocio y el contacto son obligatorios para un Shaper.", 0);

            if (contrasenia.Length < 6)
                return (false, "La contraseña debe tener al menos 6 caracteres.", 0);

            if (contrasenia != confirmarContrasenia)
                return (false, "Las contraseñas no coinciden.", 0);

            Usuario existente = _usuarioRepositorio.ObtenerPorEmail(email.Trim());
            if (existente != null)
                return (false, "Ya existe un usuario registrado con ese email.", 0);

            string hash = BCrypt.Net.BCrypt.HashPassword(contrasenia);

            Shaper nuevoShaper = new Shaper(
                email.Trim(),
                hash,
                nombre,
                pais,
                nombreDeNegosio.Trim(),
                contacto.Trim(),
                logoUrl: null); // se carga después, desde otra pantalla (edición de perfil)

            int idGenerado = _usuarioRepositorio.InsertarShaper(nuevoShaper);

            if (idGenerado <= 0)
                return (false, "Ocurrió un error al registrar el shaper. Intentá nuevamente.", 0);

            return (true, null, idGenerado);
        }

        public (bool Exito, string Error, int UsuarioId) RegistrarAdmin(
        string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasenia))
                return (false, "Completá todos los campos.", 0);

            if (contrasenia.Length < 6)
                return (false, "La contraseña debe tener al menos 6 caracteres.", 0);

            if (contrasenia != confirmarContrasenia)
                return (false, "Las contraseñas no coinciden.", 0);

            Usuario existente = _usuarioRepositorio.ObtenerPorEmail(email.Trim());
            if (existente != null)
                return (false, "Ya existe un usuario registrado con ese email.", 0);

            string hash = BCrypt.Net.BCrypt.HashPassword(contrasenia);

            Usuario nuevoUsuario = new Usuario(email.Trim(), nombre, pais, hash)
            {
                TipoDeUsuario = TipoDeUsuario.Admin
            };

            int idGenerado = _usuarioRepositorio.InsertarUsuario(nuevoUsuario);

            if (idGenerado <= 0)
                return (false, "Ocurrió un error al registrar el Administrador. Intentá nuevamente.", 0);

            return (true, null, idGenerado);
        }
        public (bool Exito, string Error) ActualizarCliente(int usuarioId, string nombre, Pais pais)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            _usuarioRepositorio.ActualizarDatosCliente(usuarioId, nombre.Trim(), pais);
            return (true, null);
        }

        public async Task<(bool Exito, string Error)> ActualizarShaper(
            int usuarioId, string nombre, Pais pais, string nombreDeNegosio, string contacto, IFormFile logo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(nombreDeNegosio) || string.IsNullOrWhiteSpace(contacto))
                return (false, "El nombre de negocio y el contacto son obligatorios.");

            string logoUrl = null; // null = "no cambió", gracias al COALESCE del repositorio
            if (logo != null && logo.Length > 0)
            {
                string nombrePublico = $"shaper_{usuarioId}_logo_{DateTime.Now.Ticks}";
                logoUrl = _cloudinarioServicio.SubirImagen(logo, nombrePublico);

                if (string.IsNullOrEmpty(logoUrl))
                    return (false, "No se pudo subir el logo. Intentá nuevamente.");
            }

            _usuarioRepositorio.ActualizarDatosShaper(
                usuarioId, nombre.Trim(), pais, nombreDeNegosio.Trim(), contacto.Trim(), logoUrl);

            return (true, null);
        }
        public Usuario BuscarPorId(int id) => _usuarioRepositorio.ObtenerPorId(id);
    }
}
