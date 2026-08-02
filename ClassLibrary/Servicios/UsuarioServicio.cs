using ClassLibrary.Datos;
using ClassLibrary.Servicios;
using ClassLibrary.Enums;
using ClassLibrary.Persona;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Servicios
{
    public interface IUsuarioServicio
    {
        Usuario Login(string email, string contrasenia);
        (bool Exito, string Error, int UsuarioId) RegistrarCliente(
        string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia);
        (bool Exito, string Error, int UsuarioId) RegistrarShaper(
        string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia,
        string nombreDeNegosio, string contacto);
        (bool Exito, string Error, int UsuarioId) RegistrarAdmin(
        string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia);
        Usuario BuscarPorId(int id);

    }
    public class UsuarioServicio : IUsuarioServicio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public UsuarioServicio(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
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

        public Usuario BuscarPorId(int id) => _usuarioRepositorio.ObtenerPorId(id);
    }
}
