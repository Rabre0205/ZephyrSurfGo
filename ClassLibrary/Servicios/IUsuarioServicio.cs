using ClassLibrary.Persona;
using ClassLibrary.Enums;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        (bool Exito, string Error) ActualizarCliente(int usuarioId, string nombre, Pais pais);

        Task<(bool Exito, string Error)> ActualizarShaper(
            int usuarioId, string nombre, Pais pais, string nombreDeNegosio, string contacto, IFormFile logo);

        Usuario BuscarPorId(int id);
    }
}
