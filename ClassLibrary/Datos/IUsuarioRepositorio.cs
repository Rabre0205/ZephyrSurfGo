using ClassLibrary.Persona;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Datos
{
    public interface IUsuarioRepositorio
    {
        List<Usuario> ObtenerTodos();
        Usuario ObtenerPorId(int id);
        Usuario ObtenerPorEmail(string email);
        int InsertarUsuario(Usuario usuario);
        int InsertarShaper(Shaper shaper);
    }
}
