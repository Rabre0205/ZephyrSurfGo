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

        List<Shaper> ObtenerShapersPaginados(
    string busqueda,
    int pagina,
    int cantidadPorPagina
);

        int ContarShapers(string busqueda);

        int ContarUsuariosPorTipo(TipoDeUsuario tipo);
        int ContarClientes(string busqueda);
        List<ClienteAdminItem> ObtenerClientesPaginados(
            string busqueda, int pagina, int cantidadPorPagina);
        bool CambiarEstadoCliente(int id, bool activo);

        int ContarShapersActivos();

        Usuario? ObtenerPorId(int id);
        Usuario? ObtenerPorEmail(string email);
        int InsertarUsuario(Usuario usuario);
        int InsertarShaper(Shaper shaper);


        bool CambiarEstadoShaper(int id, bool activo);
        bool ActualizarCuenta(int id, string email, string nombre, Pais pais);
        bool ActualizarContrasenia(int id, string contraseniaHash);
        bool ActualizarShaper(
            int id,
            string email,
            string nombre,
            Pais pais,
            string nombreDeNegosio,
            string contacto
        );
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
            LogoUrl,
            Activo";

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

        public bool ActualizarCuenta(int id, string email, string nombre, Pais pais)
        {
            const string sql = @"
                UPDATE Usuarios
                SET Email = @Email, Nombre = @Nombre, PaisId = @PaisId
                WHERE Id = @Id AND TipoDeUsuarioId <> @TipoAdministrador;";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = email;
                comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = nombre;
                comando.Parameters.Add("@PaisId", SqlDbType.Int).Value = Convert.ToInt32(pais);
                comando.Parameters.Add("@TipoAdministrador", SqlDbType.Int).Value = Convert.ToInt32(TipoDeUsuario.Administrador);
                conexion.Open();
                return comando.ExecuteNonQuery() == 1;
            }
        }

        public bool ActualizarContrasenia(int id, string contraseniaHash)
        {
            const string sql = @"
                UPDATE Usuarios
                SET Contrasenia = @Contrasenia
                WHERE Id = @Id AND TipoDeUsuarioId <> @TipoAdministrador;";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                comando.Parameters.Add("@Contrasenia", SqlDbType.NVarChar, 255).Value = contraseniaHash;
                comando.Parameters.Add("@TipoAdministrador", SqlDbType.Int).Value = Convert.ToInt32(TipoDeUsuario.Administrador);
                conexion.Open();
                return comando.ExecuteNonQuery() == 1;
            }
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

        public bool ActualizarShaper(
    int id,
    string email,
    string nombre,
    Pais pais,
    string nombreDeNegosio,
    string contacto)
        {
            string sql = @"
        UPDATE Usuarios
        SET
            Email = @Email,
            Nombre = @Nombre,
            PaisId = @PaisId,
            NombreDeNegosio = @NombreDeNegosio,
            Contacto = @Contacto
        WHERE Id = @Id
          AND TipoDeUsuarioId = @TipoShaper;
    ";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@Id",
                    SqlDbType.Int
                ).Value = id;

                comando.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    150
                ).Value = email.Trim();

                comando.Parameters.Add(
                    "@Nombre",
                    SqlDbType.NVarChar,
                    150
                ).Value = nombre.Trim();

                comando.Parameters.Add(
                    "@PaisId",
                    SqlDbType.Int
                ).Value = Convert.ToInt32(pais);

                comando.Parameters.Add(
                    "@NombreDeNegosio",
                    SqlDbType.NVarChar,
                    150
                ).Value = nombreDeNegosio.Trim();

                comando.Parameters.Add(
                    "@Contacto",
                    SqlDbType.NVarChar,
                    150
                ).Value = contacto.Trim();

                comando.Parameters.Add(
                    "@TipoShaper",
                    SqlDbType.Int
                ).Value = Convert.ToInt32(
                    TipoDeUsuario.Shaper
                );

                conexion.Open();

                int filasAfectadas =
                    comando.ExecuteNonQuery();

                return filasAfectadas > 0;
            }
        }

        public bool CambiarEstadoShaper(int id, bool activo)
        {
            string sql = @"
        UPDATE Usuarios
        SET Activo = @Activo
        WHERE Id = @Id
          AND TipoDeUsuarioId = @TipoShaper;
    ";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                comando.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;
                comando.Parameters.Add("@TipoShaper", SqlDbType.Int).Value =
                    Convert.ToInt32(TipoDeUsuario.Shaper);

                conexion.Open();

                return comando.ExecuteNonQuery() > 0;
            }
        }

        public List<Shaper> ObtenerShapersPaginados(
    string busqueda,
    int pagina,
    int cantidadPorPagina)
        {
            List<Shaper> shapers = new List<Shaper>();

            int desplazamiento =
                (pagina - 1) * cantidadPorPagina;

            string sql = $@"
        SELECT {ColumnasUsuario}
        FROM Usuarios
        WHERE TipoDeUsuarioId = @TipoShaper
          AND (
              @Busqueda = ''
              OR Nombre LIKE '%' + @Busqueda + '%'
              OR Email LIKE '%' + @Busqueda + '%'
              OR NombreDeNegosio LIKE '%' + @Busqueda + '%'
              OR Contacto LIKE '%' + @Busqueda + '%'
          )
        ORDER BY Nombre, Id
        OFFSET @Desplazamiento ROWS
        FETCH NEXT @CantidadPorPagina ROWS ONLY;
    ";

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@TipoShaper",
                    SqlDbType.Int
                ).Value = Convert.ToInt32(
                    TipoDeUsuario.Shaper
                );

                comando.Parameters.Add(
                    "@Busqueda",
                    SqlDbType.NVarChar,
                    150
                ).Value = busqueda;

                comando.Parameters.Add(
                    "@Desplazamiento",
                    SqlDbType.Int
                ).Value = desplazamiento;

                comando.Parameters.Add(
                    "@CantidadPorPagina",
                    SqlDbType.Int
                ).Value = cantidadPorPagina;

                conexion.Open();

                using (SqlDataReader lector =
                    comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        Usuario usuario =
                            MapearUsuario(lector);

                        if (usuario is Shaper shaper)
                        {
                            shapers.Add(shaper);
                        }
                    }
                }
            }

            return shapers;
        }

        public int ContarShapers(string busqueda)
        {
            string sql = @"
        SELECT COUNT(*)
        FROM Usuarios
        WHERE TipoDeUsuarioId = @TipoShaper
          AND (
              @Busqueda = ''
              OR Nombre LIKE '%' + @Busqueda + '%'
              OR Email LIKE '%' + @Busqueda + '%'
              OR NombreDeNegosio LIKE '%' + @Busqueda + '%'
              OR Contacto LIKE '%' + @Busqueda + '%'
          );
    ";

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@TipoShaper",
                    SqlDbType.Int
                ).Value = Convert.ToInt32(
                    TipoDeUsuario.Shaper
                );

                comando.Parameters.Add(
                    "@Busqueda",
                    SqlDbType.NVarChar,
                    150
                ).Value = busqueda;

                conexion.Open();

                return Convert.ToInt32(
                    comando.ExecuteScalar()
                );
            }
        }

        public int ContarUsuariosPorTipo(TipoDeUsuario tipo)
        {
            string sql = @"
        SELECT COUNT(*)
        FROM Usuarios
        WHERE TipoDeUsuarioId = @Tipo;
    ";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@Tipo",
                    SqlDbType.Int
                ).Value = Convert.ToInt32(tipo);

                conexion.Open();

                return Convert.ToInt32(
                    comando.ExecuteScalar()
                );
            }
        }

        public int ContarClientes(string busqueda)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Usuarios
                WHERE TipoDeUsuarioId = @TipoCliente
                  AND (@Busqueda = '' OR Nombre LIKE '%' + @Busqueda + '%'
                       OR Email LIKE '%' + @Busqueda + '%');";
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@TipoCliente", SqlDbType.Int).Value = (int)TipoDeUsuario.Cliente;
                comando.Parameters.Add("@Busqueda", SqlDbType.NVarChar, 150).Value = busqueda;
                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public List<ClienteAdminItem> ObtenerClientesPaginados(
            string busqueda, int pagina, int cantidadPorPagina)
        {
            var clientes = new List<ClienteAdminItem>();
            const string sql = @"
                SELECT u.Id, u.Nombre, u.Email, u.PaisId, u.Activo,
                       COUNT(p.Id) AS TotalPedidos,
                       COALESCE(SUM(p.Total), 0) AS GastoTotal
                FROM Usuarios u
                LEFT JOIN Pedidos p ON p.ClienteId = u.Id
                WHERE u.TipoDeUsuarioId = @TipoCliente
                  AND (@Busqueda = '' OR u.Nombre LIKE '%' + @Busqueda + '%'
                       OR u.Email LIKE '%' + @Busqueda + '%')
                GROUP BY u.Id, u.Nombre, u.Email, u.PaisId, u.Activo
                ORDER BY u.Nombre, u.Id
                OFFSET @Desplazamiento ROWS FETCH NEXT @Cantidad ROWS ONLY;";
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@TipoCliente", SqlDbType.Int).Value = (int)TipoDeUsuario.Cliente;
                comando.Parameters.Add("@Busqueda", SqlDbType.NVarChar, 150).Value = busqueda;
                comando.Parameters.Add("@Desplazamiento", SqlDbType.Int).Value = (pagina - 1) * cantidadPorPagina;
                comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidadPorPagina;
                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        clientes.Add(new ClienteAdminItem
                        {
                            Id = Convert.ToInt32(lector["Id"]),
                            Nombre = Convert.ToString(lector["Nombre"]) ?? string.Empty,
                            Email = Convert.ToString(lector["Email"]) ?? string.Empty,
                            Pais = (Pais)Convert.ToInt32(lector["PaisId"]),
                            Activo = Convert.ToBoolean(lector["Activo"]),
                            TotalPedidos = Convert.ToInt32(lector["TotalPedidos"]),
                            GastoTotal = Convert.ToDecimal(lector["GastoTotal"])
                        });
                    }
                }
            }
            return clientes;
        }

        public bool CambiarEstadoCliente(int id, bool activo)
        {
            const string sql = @"UPDATE Usuarios SET Activo = @Activo
                                 WHERE Id = @Id AND TipoDeUsuarioId = @TipoCliente;";
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                comando.Parameters.Add("@TipoCliente", SqlDbType.Int).Value = (int)TipoDeUsuario.Cliente;
                conexion.Open();
                return comando.ExecuteNonQuery() == 1;
            }
        }

        public int ContarShapersActivos()
        {
            string sql = @"
        SELECT COUNT(*)
        FROM Usuarios
        WHERE TipoDeUsuarioId = @TipoShaper
          AND Activo = 1;
    ";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@TipoShaper",
                    SqlDbType.Int
                ).Value = Convert.ToInt32(
                    TipoDeUsuario.Shaper
                );

                conexion.Open();

                return Convert.ToInt32(
                    comando.ExecuteScalar()
                );
            }
        }


        private Usuario MapearUsuario(SqlDataReader lector)
        {
            int id = Convert.ToInt32(lector["Id"]);

            string email =
                Convert.ToString(lector["Email"])
                ?? string.Empty;

            string contrasenia =
                Convert.ToString(lector["Contrasenia"])
                ?? string.Empty;

            string nombre =
                Convert.ToString(lector["Nombre"])
                ?? string.Empty;

            Pais pais =
                (Pais)Convert.ToInt32(
                    lector["PaisId"]
                );

            TipoDeUsuario tipo =
                (TipoDeUsuario)Convert.ToInt32(
                    lector["TipoDeUsuarioId"]
                );

            bool activo =
                Convert.ToBoolean(
                    lector["Activo"]
                );

            if (tipo == TipoDeUsuario.Shaper)
            {
                string nombreDeNegosio =
                    lector["NombreDeNegosio"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(
                            lector["NombreDeNegosio"]
                        ) ?? string.Empty;

                string contacto =
                    lector["Contacto"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(
                            lector["Contacto"]
                        ) ?? string.Empty;

                string logoUrl =
                    lector["LogoUrl"] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(
                            lector["LogoUrl"]
                        ) ?? string.Empty;

                Shaper shaper = new Shaper(
                    id,
                    email,
                    contrasenia,
                    nombre,
                    pais,
                    nombreDeNegosio,
                    contacto,
                    logoUrl
                );

                shaper.Activo = activo;

                return shaper;
            }

            Usuario usuario = new Usuario(
                id,
                email,
                nombre,
                pais,
                contrasenia
            );

            usuario.TipoDeUsuario = tipo;
            usuario.Activo = activo;

            return usuario;
        }
    }
}
