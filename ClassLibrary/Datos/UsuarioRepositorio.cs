using ClassLibrary.Enums;
using ClassLibrary.Persona;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private const string ColumnasUsuario = @"
            Id,
            Email,
            Contrasenia,
            Nombre,
            PaisId,
            TipoDeUsuarioId,
            NombreDeNegosio,
            Contacto,
            LogoUrl";

        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> usuarios = new List<Usuario>();

            string sql = $@"
                SELECT {ColumnasUsuario}
                FROM Usuarios;";

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

        public Usuario? ObtenerPorId(int id)
        {
            string sql = $@"
                SELECT {ColumnasUsuario}
                FROM Usuarios
                WHERE Id = @Id;";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        return MapearUsuario(lector);
                    }
                }
            }

            return null;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            string sql = $@"
                SELECT {ColumnasUsuario}
                FROM Usuarios
                WHERE LOWER(Email) = LOWER(@Email);";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
                    .Value = email.Trim();

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        return MapearUsuario(lector);
                    }
                }
            }

            return null;
        }

        public Usuario? Login(string email, string contrasenia)
        {
            string sql = $@"
                SELECT {ColumnasUsuario}
                FROM Usuarios
                WHERE LOWER(Email) = LOWER(@Email)
                  AND Contrasenia = @Contrasenia;";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
                    .Value = email.Trim();

                comando.Parameters.Add("@Contrasenia", SqlDbType.NVarChar, 255)
                    .Value = contrasenia;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        return MapearUsuario(lector);
                    }
                }
            }

            return null;
        }

        public int InsertarUsuario(Usuario usuario)
        {
            string sql = @"
    INSERT INTO Usuarios
    (
        Email,
        Contrasenia,
        Nombre,
        PaisId,
        TipoDeUsuarioId
    )
    VALUES
    (
        @Email,
        @Contrasenia,
        @Nombre,
        @PaisId,
        @TipoDeUsuarioId
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);
";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
                    .Value = usuario.Email;

                comando.Parameters.Add("@Contrasenia", SqlDbType.NVarChar, 255)
                    .Value = usuario.Contrasenia;

                comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150)
                    .Value = usuario.Nombre;

                comando.Parameters.Add("@PaisId", SqlDbType.Int)
                    .Value = Convert.ToInt32(usuario.Pais);

                comando.Parameters.Add("@TipoDeUsuarioId", SqlDbType.Int)
                    .Value = Convert.ToInt32(usuario.TipoDeUsuario);

                conexion.Open();

                object? resultado = comando.ExecuteScalar();

                if (resultado == null)
                {
                    throw new InvalidOperationException(
                        "No se pudo obtener el ID del usuario insertado.");
                }

                return Convert.ToInt32(resultado);
            }
        }

        public int InsertarShaper(Shaper shaper)
        {
            string sql = @"
    INSERT INTO Usuarios
    (
        Email,
        Contrasenia,
        Nombre,
        PaisId,
        TipoDeUsuarioId,
        NombreDeNegosio,
        Contacto,
        LogoUrl
    )
    VALUES
    (
        @Email,
        @Contrasenia,
        @Nombre,
        @PaisId,
        @TipoDeUsuarioId,
        @NombreDeNegosio,
        @Contacto,
        @LogoUrl
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);
";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150)
                    .Value = shaper.Email;

                comando.Parameters.Add("@Contrasenia", SqlDbType.NVarChar, 255)
                    .Value = shaper.Contrasenia;

                comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150)
                    .Value = shaper.Nombre;

                comando.Parameters.Add("@PaisId", SqlDbType.Int)
                    .Value = Convert.ToInt32(shaper.Pais);

                comando.Parameters.Add("@TipoDeUsuarioId", SqlDbType.Int)
                    .Value = Convert.ToInt32(TipoDeUsuario.Shaper);

                comando.Parameters.Add("@NombreDeNegosio", SqlDbType.NVarChar, 150)
                    .Value = string.IsNullOrWhiteSpace(shaper.NombreDeNegosio)
                        ? DBNull.Value
                        : shaper.NombreDeNegosio;

                comando.Parameters.Add("@Contacto", SqlDbType.NVarChar, 150)
                    .Value = string.IsNullOrWhiteSpace(shaper.Contacto)
                        ? DBNull.Value
                        : shaper.Contacto;

                comando.Parameters.Add("@LogoUrl", SqlDbType.NVarChar, 500)
                    .Value = string.IsNullOrWhiteSpace(shaper.LogoUrl)
                        ? DBNull.Value
                        : shaper.LogoUrl;

                conexion.Open();

                object? resultado = comando.ExecuteScalar();

                if (resultado == null)
                {
                    throw new InvalidOperationException(
                        "No se pudo obtener el ID del shaper insertado.");
                }

                return Convert.ToInt32(resultado);
            }
        }

        private Usuario MapearUsuario(SqlDataReader lector)
        {
            int id = Convert.ToInt32(lector["Id"]);
            string email = Convert.ToString(lector["Email"]) ?? string.Empty;
            string contrasenia =
                Convert.ToString(lector["Contrasenia"]) ?? string.Empty;
            string nombre = Convert.ToString(lector["Nombre"]) ?? string.Empty;

            Pais pais = (Pais)Convert.ToInt32(lector["PaisId"]);

            TipoDeUsuario tipo =
                (TipoDeUsuario)Convert.ToInt32(lector["TipoDeUsuarioId"]);

            if (tipo == TipoDeUsuario.Shaper)
            {
                string nombreDeNegosio =
                    lector["NombreDeNegosio"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(lector["NombreDeNegosio"])
                            ?? string.Empty;

                string contacto =
                    lector["Contacto"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(lector["Contacto"])
                            ?? string.Empty;

                string logoUrl =
                    lector["LogoUrl"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(lector["LogoUrl"])
                            ?? string.Empty;

                return new Shaper(
                    id,
                    email,
                    contrasenia,
                    nombre,
                    pais,
                    nombreDeNegosio,
                    contacto,
                    logoUrl
                );
            }

            return new Usuario(
                id,
                email,
                nombre,
                pais,
                contrasenia
            );
        }
    }
}
 