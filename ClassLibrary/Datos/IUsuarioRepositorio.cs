using System.Collections.Generic;
using ClassLibrary.Persona;
using ClassLibrary.Enums;

namespace ClassLibrary.Datos
{
    public interface IUsuarioRepositorio
    {
        List<Usuario> ObtenerTodos();
        Usuario ObtenerPorId(int id);
        Usuario ObtenerPorEmail(string email);
        int InsertarUsuario(Usuario usuario);
        int InsertarShaper(Shaper shaper);
        void ActualizarDatosCliente(int usuarioId, string nombre, Pais pais);
        void ActualizarDatosShaper(int usuarioId, string nombre, Pais pais, string nombreDeNegosio, string contacto, string logoUrl);
    }
}
