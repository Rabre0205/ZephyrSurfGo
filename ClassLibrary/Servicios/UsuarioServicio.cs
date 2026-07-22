using ClassLibrary.Datos;
using ClassLibrary.Persona;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Servicios
{
    public interface IUsuarioServicio
    {
        Usuario Login(string email, string contrasenia);
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

        public Usuario BuscarPorId(int id) => _usuarioRepositorio.ObtenerPorId(id);
    }
}
