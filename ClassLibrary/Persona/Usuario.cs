using ClassLibrary.Enums;

namespace ClassLibrary.Persona
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public bool Activo { get; set; } = true;
        public string Contrasenia { get; set; }

        public string Nombre { get; set; }

        public Pais Pais { get; set; }

        public TipoDeUsuario TipoDeUsuario { get; set; }

        public Usuario(
            int id,
            string email,
            string nombre,
            Pais pais,
            string contrasenia)
        {
            Id = id;
            Email = email;
            Nombre = nombre;
            Pais = pais;
            Contrasenia = contrasenia;
            TipoDeUsuario = TipoDeUsuario.Cliente;
        }

        public Usuario(
            string email,
            string nombre,
            Pais pais,
            string contrasenia)
        {
            Email = email;
            Nombre = nombre;
            Pais = pais;
            Contrasenia = contrasenia;
            TipoDeUsuario = TipoDeUsuario.Cliente;
        }
    }
}