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
        Usuario BuscarPorId(int id);
        int RegistrarUsuario(Usuario usuario, string contraseniaPlano);
        int RegistrarShaper(Shaper shaper, string contraseniaPlano);
    }
    public class UsuarioServicio : IUsuarioServicio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public UsuarioServicio(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }
        public int RegistrarUsuario(Usuario usuario, string contraseniaPlano)
        {
            usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(contraseniaPlano);
            return _usuarioRepositorio.InsertarUsuario(usuario);
        }

        public int RegistrarShaper(Shaper shaper, string contraseniaPlano)
        {
            shaper.Contrasenia = BCrypt.Net.BCrypt.HashPassword(contraseniaPlano);
            return _usuarioRepositorio.InsertarShaper(shaper);
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

        public Usuario BuscarPorId(int id) => _usuarioRepositorio.ObtenerPorId(id);
    }
}
