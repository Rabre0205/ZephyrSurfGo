using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ClassLibrary.Persona;
using ClassLibrary.Enums;

namespace ClassLibrary.Datos
{
    public class UsuarioRepositorio
    {
        /// <summary>
        /// Trae todos los usuarios. Si TipoDeUsuario = Shaper, instancia un Shaper;
        /// si no, instancia un Usuario base.
        /// </summary>
        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> usuarios = new List<Usuario>();

            string sql = @"SELECT Id, Email, Contrasenia, Nombre, PaisId, TipoDeUsuarioId,
                            NombreDeNegosio, Contacto, LogoUrl
                            FROM Usuarios";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        usuarios.Add(MapearUsuario(lector));
                    }
                }
            }

            return usuarios;
        }

        public Usuario ObtenerPorId(int id)
        {
            Usuario usuario = null;

            string sql = @"SELECT Id, Email, Contrasenia, Nombre, Pais, TipoDeUsuario,
                                   NombreDeNegosio, Contacto, LogoUrl
                            FROM Usuarios
                            WHERE Id = @Id";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        usuario = MapearUsuario(lector);
                    }
                }
            }

            return usuario;
        }

        public Usuario ObtenerPorEmail(string email)
        {
            Usuario usuario = null;

            string sql = @"SELECT Id, Email, Contrasenia, Nombre, Pais, TipoDeUsuario,
                                   NombreDeNegosio, Contacto, LogoUrl
                            FROM Usuarios
                            WHERE Email = @Email";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = email;
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        usuario = MapearUsuario(lector);
                    }
                }
            }

            return usuario;
        }

        /// <summary>
        /// Inserta un Cliente (Usuario base) y devuelve el Id generado.
        /// </summary>
        public int InsertarUsuario(Usuario usuario)
        {
            string sql = @"INSERT INTO Usuarios (Email, Contrasenia, Nombre, Pais, TipoDeUsuario)
                            OUTPUT INSERTED.Id
                            VALUES (@Email, @Contrasenia, @Nombre, @Pais, @TipoDeUsuario)";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = usuario.Email;
                comando.Parameters.Add("@Contrasenia", SqlDbType.NVarChar, 255).Value = usuario.Contrasenia;
                comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = usuario.Nombre;
                comando.Parameters.Add("@Pais", SqlDbType.TinyInt).Value = (byte)usuario.Pais;
                comando.Parameters.Add("@TipoDeUsuario", SqlDbType.TinyInt).Value = (byte)usuario.TipoDeUsuario;

                conexion.Open();
                return (int)comando.ExecuteScalar();
            }
        }

        /// <summary>
        /// Inserta un Shaper (incluye columnas propias de la subclase).
        /// </summary>
        public int InsertarShaper(Shaper shaper)
        {
            string sql = @"INSERT INTO Usuarios
                                (Email, Contrasenia, Nombre, Pais, TipoDeUsuario,
                                 NombreDeNegosio, Contacto, LogoUrl)
                            OUTPUT INSERTED.Id
                            VALUES
                                (@Email, @Contrasenia, @Nombre, @Pais, @TipoDeUsuario,
                                 @NombreDeNegosio, @Contacto, @LogoUrl)";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = shaper.Email;
                comando.Parameters.Add("@Contrasenia", SqlDbType.NVarChar, 255).Value = shaper.Contrasenia;
                comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = shaper.Nombre;
                comando.Parameters.Add("@Pais", SqlDbType.TinyInt).Value = (byte)shaper.Pais;
                comando.Parameters.Add("@TipoDeUsuario", SqlDbType.TinyInt).Value = (byte)TipoDeUsuario.Shaper;
                comando.Parameters.Add("@NombreDeNegosio", SqlDbType.NVarChar, 150).Value = shaper.NombreDeNegosio ?? (object)DBNull.Value;
                comando.Parameters.Add("@Contacto", SqlDbType.NVarChar, 150).Value = shaper.Contacto ?? (object)DBNull.Value;
                comando.Parameters.Add("@LogoUrl", SqlDbType.NVarChar, 500).Value = shaper.LogoUrl ?? (object)DBNull.Value;

                conexion.Open();
                return (int)comando.ExecuteScalar();
            }
        }

        /// <summary>
        /// Mapea una fila del reader a Usuario o Shaper según TipoDeUsuario.
        /// Sin LINQ: lectura secuencial por índice/nombre de columna.
        /// </summary>
        private Usuario MapearUsuario(SqlDataReader lector)
        {
            int id = lector.GetInt32(lector.GetOrdinal("Id"));
            string email = lector.GetString(lector.GetOrdinal("Email"));
            string contrasenia = lector.GetString(lector.GetOrdinal("Contrasenia"));
            string nombre = lector.GetString(lector.GetOrdinal("Nombre"));
            Pais pais = (Pais)lector.GetByte(lector.GetOrdinal("PaisId"));
            TipoDeUsuario tipo = (TipoDeUsuario)lector.GetByte(lector.GetOrdinal("TipoDeUsuarioId"));

            if (tipo == TipoDeUsuario.Shaper)
            {
                int ordinalNegocio = lector.GetOrdinal("NombreDeNegosio");
                int ordinalContacto = lector.GetOrdinal("Contacto");
                int ordinalLogo = lector.GetOrdinal("LogoUrl");

                string nombreDeNegosio = lector.IsDBNull(ordinalNegocio) ? string.Empty : lector.GetString(ordinalNegocio);
                string contacto = lector.IsDBNull(ordinalContacto) ? string.Empty : lector.GetString(ordinalContacto);
                string logoUrl = lector.IsDBNull(ordinalLogo) ? string.Empty : lector.GetString(ordinalLogo);

                return new Shaper(id, email, contrasenia, nombre, pais, nombreDeNegosio, contacto, logoUrl);
            }

            return new Usuario(id, email, nombre, pais, contrasenia);
        }
    }
}
