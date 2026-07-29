using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ClassLibrary.Productos;
using ClassLibrary.Enums;

namespace ClassLibrary.Datos
{
    public interface IProductoRepositorio
    {
        List<Producto> ObtenerTodos();
        List<Producto> ObtenerPorShaper(int shaperId);

        Producto ObtenerPorId(int id);

        int ActualizarTabla(Tabla tabla);

        bool EliminarTabla(int id, int shaperId);

        int InsertarLeash(Leash leash);
        int InsertarPad(Pad pad);
        int InsertarQuilla(Quilla quilla);
        int InsertarTabla(Tabla tabla);
        int InsertarTraje(Traje traje);
        bool ReservarTabla(int productoId, SqlConnection conexion, SqlTransaction transaccion);
        bool DescontarStock(int productoId, int cantidad, string tipoProducto, SqlConnection conexion, SqlTransaction transaccion);
    }

    public class ProductoRepositorio : IProductoRepositorio
    {
        /// <summary>
        /// Trae todos los productos. Como es TPT, se hace LEFT JOIN a las 5 tablas hijas
        /// y se instancia la subclase correcta según la columna TipoProducto.
        /// </summary>
        public List<Producto> ObtenerTodos()
        {
            List<Producto> productos = new List<Producto>();

            string sql = @"
                        SELECT
                            p.Id, p.Titulo, p.Subtitulo, p.Precio, p.Descripcion, p.ImagenUrl,
                            p.ShaperId, p.TipoProducto,
                            l.LargoDeTablaRecomendado,
                            pa.Largo, pa.Ancho AS AnchoPad, pa.Material,
                            q.SistemaDeEncajeId AS SistemaEncajeQuilla,
                            t.Altura, t.Ancho AS AnchoTabla, t.Volumen, t.SistemaDeEncajeId AS SistemaEncajeTabla,
                            t.TipoDeOlaId, t.EstiloDeSurfId, t.PesoMinimo, t.PesoMaximo,
                            t.ExperienciaId, t.ImagenAtrasUrl,
                            tr.GeneroId, tr.Espesor, tr.TalleId, tr.Temperatura
                        FROM Productos p
                        LEFT JOIN Leashes l ON l.ProductoId = p.Id
                        LEFT JOIN Pads    pa ON pa.ProductoId = p.Id
                        LEFT JOIN Quillas q ON q.ProductoId = p.Id
                        LEFT JOIN Tablas  t ON t.ProductoId = p.Id
                        LEFT JOIN Trajes  tr ON tr.ProductoId = p.Id";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        productos.Add(MapearProducto(lector));
                    }
                }
            }

            return productos;
        }

        // nuevo método en ProductoRepositorio, mismo estilo que ObtenerTodos()
        public List<Producto> ObtenerPorShaper(int shaperId)
        {
            List<Producto> productos = new List<Producto>();

            string sql = @"
            SELECT
                p.Id, p.Titulo, p.Subtitulo, p.Precio, p.Descripcion, p.ImagenUrl,
                p.ShaperId, p.TipoProducto,
                l.LargoDeTablaRecomendado,
                pa.Largo, pa.Ancho AS AnchoPad, pa.Material,
                q.SistemaDeEncajeId AS SistemaEncajeQuilla,
                t.Altura, t.Ancho AS AnchoTabla, t.Volumen, t.SistemaDeEncajeId AS SistemaEncajeTabla,
                t.TipoDeOlaId, t.EstiloDeSurfId, t.PesoMinimo, t.PesoMaximo,
                t.ExperienciaId, t.ImagenAtrasUrl,
                tr.GeneroId, tr.Espesor, tr.TalleId, tr.Temperatura
            FROM Productos p
            LEFT JOIN Leashes l ON l.ProductoId = p.Id
            LEFT JOIN Pads    pa ON pa.ProductoId = p.Id
            LEFT JOIN Quillas q ON q.ProductoId = p.Id
            LEFT JOIN Tablas  t ON t.ProductoId = p.Id
            LEFT JOIN Trajes  tr ON tr.ProductoId = p.Id
            WHERE p.ShaperId = @ShaperId";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        productos.Add(MapearProducto(lector));
                }
            }

            return productos;
        }

        public Producto ObtenerPorId(int id)
        {
            string sql = @"
        SELECT
            p.Id,
            p.Titulo,
            p.Subtitulo,
            p.Precio,
            p.Descripcion,
            p.ImagenUrl,
            p.ShaperId,
            p.TipoProducto,

            l.LargoDeTablaRecomendado,

            pa.Largo,
            pa.Ancho AS AnchoPad,
            pa.Material,

            q.SistemaDeEncajeId AS SistemaEncajeQuilla,

            t.Altura,
            t.Ancho AS AnchoTabla,
            t.Volumen,
            t.SistemaDeEncajeId AS SistemaEncajeTabla,
            t.TipoDeOlaId,
            t.EstiloDeSurfId,
            t.PesoMinimo,
            t.PesoMaximo,
            t.ExperienciaId,
            t.ImagenAtrasUrl,

            tr.GeneroId,
            tr.Espesor,
            tr.TalleId,
            tr.Temperatura

        FROM Productos p
        LEFT JOIN Leashes l ON l.ProductoId = p.Id
        LEFT JOIN Pads pa ON pa.ProductoId = p.Id
        LEFT JOIN Quillas q ON q.ProductoId = p.Id
        LEFT JOIN Tablas t ON t.ProductoId = p.Id
        LEFT JOIN Trajes tr ON tr.ProductoId = p.Id

        WHERE p.Id = @Id;
    ";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        return MapearProducto(lector);
                    }
                }
            }

            return null;
        }

        private Producto MapearProducto(SqlDataReader lector)
        {
            int id = lector.GetInt32(lector.GetOrdinal("Id"));
            string titulo = lector.GetString(lector.GetOrdinal("Titulo"));
            int ordSubtitulo = lector.GetOrdinal("Subtitulo");
            string subtitulo = lector.IsDBNull(ordSubtitulo) ? string.Empty : lector.GetString(ordSubtitulo);
            double precio = (double)lector.GetDecimal(lector.GetOrdinal("Precio"));
            int ordDescripcion = lector.GetOrdinal("Descripcion");
            string descripcion = lector.IsDBNull(ordDescripcion) ? string.Empty : lector.GetString(ordDescripcion);
            int ordImagen = lector.GetOrdinal("ImagenUrl");
            string imagenUrl = lector.IsDBNull(ordImagen) ? string.Empty : lector.GetString(ordImagen);
            int shaperId = lector.GetInt32(lector.GetOrdinal("ShaperId"));
            string tipoProducto = lector.GetString(lector.GetOrdinal("TipoProducto"));

            switch (tipoProducto)
            {
                case "Leash":
                    int largo = lector.GetInt32(lector.GetOrdinal("LargoDeTablaRecomendado"));
                    return new Leash(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId, largo);

                case "Pad":
                    int ordLargoPad = lector.GetOrdinal("Largo");
                    int ordAnchoPad = lector.GetOrdinal("AnchoPad");
                    int ordMaterial = lector.GetOrdinal("Material");
                    return new Pad(
                        id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId,
                        lector.GetInt32(ordLargoPad),
                        lector.GetInt32(ordAnchoPad),
                        lector.GetString(ordMaterial));

                case "Quilla":
                    int ordSistemaQuilla = lector.GetOrdinal("SistemaEncajeQuilla");
                    SistemaDeEncaje sistemaQuilla =
                        (SistemaDeEncaje)lector.GetByte(ordSistemaQuilla);
                    return new Quilla(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId, sistemaQuilla);

                case "Tabla":
                    int ordAltura = lector.GetOrdinal("Altura");
                    string altura = lector.IsDBNull(ordAltura) ? string.Empty : lector.GetString(ordAltura);
                    int anchoTabla = lector.GetInt32(lector.GetOrdinal("AnchoTabla"));
                    double volumen = (double)lector.GetDecimal(lector.GetOrdinal("Volumen"));
                    int ordSistemaTabla = lector.GetOrdinal("SistemaEncajeTabla");
                    SistemaDeEncaje sistemaTabla =
                        (SistemaDeEncaje)lector.GetByte(ordSistemaTabla);
                    TipoDeOla tipoOla = (TipoDeOla)lector.GetByte(lector.GetOrdinal("TipoDeOlaId"));
                    EstiloDeSurf estilo = (EstiloDeSurf)lector.GetByte(lector.GetOrdinal("EstiloDeSurfId"));
                    int pesoMin = lector.GetInt32(lector.GetOrdinal("PesoMinimo"));
                    int pesoMax = lector.GetInt32(lector.GetOrdinal("PesoMaximo"));
                    Experiencia experiencia = (Experiencia)lector.GetByte(lector.GetOrdinal("ExperienciaId"));
                    int ordImagenAtras = lector.GetOrdinal("ImagenAtrasUrl");
                    string imagenAtras = lector.IsDBNull(ordImagenAtras) ? string.Empty : lector.GetString(ordImagenAtras);

                    return new Tabla(
                        id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId,
                        altura, anchoTabla, volumen, sistemaTabla, tipoOla, estilo,
                        pesoMin, pesoMax, experiencia, imagenAtras);

                case "Traje":
                    Genero genero = (Genero)lector.GetByte(lector.GetOrdinal("GeneroId"));
                    int espesor = lector.GetInt32(lector.GetOrdinal("Espesor"));
                    Talle talle = (Talle)lector.GetByte(lector.GetOrdinal("TalleId"));
                    int ordTemperatura = lector.GetOrdinal("Temperatura");
                    string temperatura = lector.IsDBNull(ordTemperatura) ? string.Empty : lector.GetString(ordTemperatura);

                    return new Traje(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId,
                        genero, espesor, talle, temperatura);

                default:
                    return new Producto(id, titulo, subtitulo, precio, descripcion, imagenUrl, shaperId);
            }
        }

        /// <summary>
        /// Inserta un Leash: primero la fila base en Productos, luego la fila hija.
        /// Ambos INSERT en una misma transacción para mantener consistencia TPT.
        /// </summary>
        public int InsertarLeash(Leash leash)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    int idGenerado;

                    string sqlBase = @"INSERT INTO Productos
                                            (Titulo, Subtitulo, Precio, Descripcion, ImagenUrl, ShaperId, TipoProducto)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Titulo, @Subtitulo, @Precio, @Descripcion, @ImagenUrl, @ShaperId, 'Leash')";

                    using (SqlCommand comandoBase = new SqlCommand(sqlBase, conexion, transaccion))
                    {
                        comandoBase.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = leash.Titulo;
                        comandoBase.Parameters.Add("@Subtitulo", SqlDbType.NVarChar, 200).Value = leash.Subtitulo ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@Precio", SqlDbType.Decimal).Value = (decimal)leash.Precio;
                        comandoBase.Parameters.Add("@Descripcion", SqlDbType.NVarChar).Value = leash.Descripcion ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ImagenUrl", SqlDbType.NVarChar, 500).Value = leash.ImagenUrl ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ShaperId", SqlDbType.Int).Value = leash.ShaperId;

                        idGenerado = (int)comandoBase.ExecuteScalar();
                    }

                    string sqlHija = @"INSERT INTO Leashes (ProductoId, LargoDeTablaRecomendado)
                                        VALUES (@ProductoId, @Largo)";

                    using (SqlCommand comandoHija = new SqlCommand(sqlHija, conexion, transaccion))
                    {
                        comandoHija.Parameters.Add("@ProductoId", SqlDbType.Int).Value = idGenerado;
                        comandoHija.Parameters.Add("@Largo", SqlDbType.Int).Value = leash.LargoDeTablaRecomendado;

                        comandoHija.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return idGenerado;
                }
            }
        }

        /// <summary>
        /// Inserta un Pad siguiendo el patrón TPT.
        /// </summary>
        public int InsertarPad(Pad pad)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    int idGenerado;

                    string sqlBase = @"INSERT INTO Productos
                                            (Titulo, Subtitulo, Precio, Descripcion, ImagenUrl, ShaperId, TipoProducto)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Titulo, @Subtitulo, @Precio, @Descripcion, @ImagenUrl, @ShaperId, 'Pad')";

                    using (SqlCommand comandoBase = new SqlCommand(sqlBase, conexion, transaccion))
                    {
                        comandoBase.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = pad.Titulo;
                        comandoBase.Parameters.Add("@Subtitulo", SqlDbType.NVarChar, 200).Value = pad.Subtitulo ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@Precio", SqlDbType.Decimal).Value = (decimal)pad.Precio;
                        comandoBase.Parameters.Add("@Descripcion", SqlDbType.NVarChar).Value = pad.Descripcion ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ImagenUrl", SqlDbType.NVarChar, 500).Value = pad.ImagenUrl ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ShaperId", SqlDbType.Int).Value = pad.ShaperId;

                        idGenerado = (int)comandoBase.ExecuteScalar();
                    }

                    string sqlHija = @"INSERT INTO Pads (ProductoId, Largo, Ancho, Material)
                                        VALUES (@ProductoId, @Largo, @Ancho, @Material)";

                    using (SqlCommand comandoHija = new SqlCommand(sqlHija, conexion, transaccion))
                    {
                        comandoHija.Parameters.Add("@ProductoId", SqlDbType.Int).Value = idGenerado;
                        comandoHija.Parameters.Add("@Largo", SqlDbType.Int).Value = pad.Largo;
                        comandoHija.Parameters.Add("@Ancho", SqlDbType.Int).Value = pad.Ancho;
                        comandoHija.Parameters.Add("@Material", SqlDbType.NVarChar, 100).Value = pad.Material ?? (object)DBNull.Value;

                        comandoHija.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return idGenerado;
                }
            }
        }

        /// <summary>
        /// Inserta una Quilla siguiendo el patrón TPT.
        /// </summary>
        public int InsertarQuilla(Quilla quilla)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    int idGenerado;

                    string sqlBase = @"INSERT INTO Productos
                                            (Titulo, Subtitulo, Precio, Descripcion, ImagenUrl, ShaperId, TipoProducto)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Titulo, @Subtitulo, @Precio, @Descripcion, @ImagenUrl, @ShaperId, 'Quilla')";

                    using (SqlCommand comandoBase = new SqlCommand(sqlBase, conexion, transaccion))
                    {
                        comandoBase.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = quilla.Titulo;
                        comandoBase.Parameters.Add("@Subtitulo", SqlDbType.NVarChar, 200).Value = quilla.Subtitulo ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@Precio", SqlDbType.Decimal).Value = (decimal)quilla.Precio;
                        comandoBase.Parameters.Add("@Descripcion", SqlDbType.NVarChar).Value = quilla.Descripcion ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ImagenUrl", SqlDbType.NVarChar, 500).Value = quilla.ImagenUrl ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ShaperId", SqlDbType.Int).Value = quilla.ShaperId;

                        idGenerado = (int)comandoBase.ExecuteScalar();
                    }

                    string sqlHija = @"INSERT INTO Quillas (ProductoId, SistemaDeEncaje)
                                        VALUES (@ProductoId, @SistemaDeEncaje)";

                    using (SqlCommand comandoHija = new SqlCommand(sqlHija, conexion, transaccion))
                    {
                        comandoHija.Parameters.Add("@ProductoId", SqlDbType.Int).Value = idGenerado;
                        comandoHija.Parameters.Add("@SistemaDeEncaje", SqlDbType.TinyInt).Value = (byte)quilla.SistemaDeEncaje;

                        comandoHija.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return idGenerado;
                }
            }
        }

        /// <summary>
        /// Inserta una Tabla siguiendo el patrón TPT.
        /// </summary>
        public int InsertarTabla(Tabla tabla)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    int idGenerado;

                    string sqlBase = @"
                DECLARE @Insertados TABLE (Id INT);

                INSERT INTO Productos
                    (Titulo, Subtitulo, Precio, Descripcion, ImagenUrl, ShaperId, TipoProducto)
                OUTPUT INSERTED.Id INTO @Insertados
                VALUES (@Titulo, @Subtitulo, @Precio, @Descripcion, @ImagenUrl, @ShaperId, 'Tabla');

                SELECT Id FROM @Insertados;";

                    using (SqlCommand comandoBase = new SqlCommand(sqlBase, conexion, transaccion))
                    {
                        comandoBase.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = tabla.Titulo;
                        comandoBase.Parameters.Add("@Subtitulo", SqlDbType.NVarChar, 200).Value = tabla.Subtitulo ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@Precio", SqlDbType.Decimal).Value = (decimal)tabla.Precio;
                        comandoBase.Parameters.Add("@Descripcion", SqlDbType.NVarChar).Value = tabla.Descripcion ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ImagenUrl", SqlDbType.NVarChar, 500).Value = tabla.ImagenUrl ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ShaperId", SqlDbType.Int).Value = tabla.ShaperId;

                        idGenerado = (int)comandoBase.ExecuteScalar();
                    }

                    string sqlHija = @"INSERT INTO Tablas
                                    (ProductoId, Altura, Ancho, Volumen, SistemaDeEncajeId,
                                     TipoDeOlaId, EstiloDeSurfId, PesoMinimo, PesoMaximo,
                                     ExperienciaId, ImagenAtrasUrl)
                                VALUES
                                    (@ProductoId, @Altura, @Ancho, @Volumen, @SistemaDeEncajeId,
                                     @TipoDeOlaId, @EstiloDeSurfId, @PesoMinimo, @PesoMaximo,
                                     @ExperienciaId, @ImagenAtrasUrl)";

                    using (SqlCommand comandoHija = new SqlCommand(sqlHija, conexion, transaccion))
                    {
                        comandoHija.Parameters.Add("@ProductoId", SqlDbType.Int).Value = idGenerado;
                        comandoHija.Parameters.Add("@Altura", SqlDbType.NVarChar, 50).Value = tabla.Altura ?? (object)DBNull.Value;
                        comandoHija.Parameters.Add("@Ancho", SqlDbType.Int).Value = tabla.Ancho;
                        comandoHija.Parameters.Add("@Volumen", SqlDbType.Decimal).Value = (decimal)tabla.Volumen;
                        comandoHija.Parameters.Add("@SistemaDeEncajeId", SqlDbType.TinyInt).Value = (byte)tabla.SistemaDeEncaje;
                        comandoHija.Parameters.Add("@TipoDeOlaId", SqlDbType.TinyInt).Value = (byte)tabla.TipoDeOla;
                        comandoHija.Parameters.Add("@EstiloDeSurfId", SqlDbType.TinyInt).Value = (byte)tabla.EstiloDeSurf;
                        comandoHija.Parameters.Add("@PesoMinimo", SqlDbType.Int).Value = tabla.PesoMinimo;
                        comandoHija.Parameters.Add("@PesoMaximo", SqlDbType.Int).Value = tabla.PesoMaximo;
                        comandoHija.Parameters.Add("@ExperienciaId", SqlDbType.TinyInt).Value = (byte)tabla.Experiencia;
                        comandoHija.Parameters.Add("@ImagenAtrasUrl", SqlDbType.NVarChar, 500).Value = tabla.ImagenAtrasUrl ?? (object)DBNull.Value;

                        comandoHija.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return idGenerado;
                }
            }
        }

        public int ActualizarTabla(Tabla tabla)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    string sqlProducto = @"
            UPDATE Productos
            SET
                Titulo = @Titulo,
                Subtitulo = @Subtitulo,
                Precio = @Precio,
                Descripcion = @Descripcion,
                ImagenUrl = @ImagenUrl
            WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(sqlProducto, conexion, transaccion))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = tabla.Id;
                        cmd.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = tabla.Titulo;
                        cmd.Parameters.Add("@Subtitulo", SqlDbType.NVarChar, 200).Value =
                            (object?)tabla.Subtitulo ?? DBNull.Value;
                        cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = (decimal)tabla.Precio;
                        cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar).Value =
                            (object?)tabla.Descripcion ?? DBNull.Value;
                        cmd.Parameters.Add("@ImagenUrl", SqlDbType.NVarChar, 500).Value =
                            (object?)tabla.ImagenUrl ?? DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }

                    string sqlTabla = @"
            UPDATE Tablas
            SET
                Altura = @Altura,
                Ancho = @Ancho,
                Volumen = @Volumen,
                SistemaDeEncajeId = @SistemaDeEncajeId,
                TipoDeOlaId = @TipoDeOlaId,
                EstiloDeSurfId = @EstiloDeSurfId,
                PesoMinimo = @PesoMinimo,
                PesoMaximo = @PesoMaximo,
                ExperienciaId = @ExperienciaId,
                ImagenAtrasUrl = @ImagenAtrasUrl
            WHERE ProductoId = @Id";

                    using (SqlCommand cmd = new SqlCommand(sqlTabla, conexion, transaccion))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = tabla.Id;
                        cmd.Parameters.Add("@Altura", SqlDbType.NVarChar, 50).Value =
                            (object?)tabla.Altura ?? DBNull.Value;
                        cmd.Parameters.Add("@Ancho", SqlDbType.Int).Value = tabla.Ancho;
                        cmd.Parameters.Add("@Volumen", SqlDbType.Decimal).Value = (decimal)tabla.Volumen;
                        cmd.Parameters.Add("@SistemaDeEncajeId", SqlDbType.TinyInt).Value = (byte)tabla.SistemaDeEncaje;
                        cmd.Parameters.Add("@TipoDeOlaId", SqlDbType.TinyInt).Value = (byte)tabla.TipoDeOla;
                        cmd.Parameters.Add("@EstiloDeSurfId", SqlDbType.TinyInt).Value = (byte)tabla.EstiloDeSurf;
                        cmd.Parameters.Add("@PesoMinimo", SqlDbType.Int).Value = tabla.PesoMinimo;
                        cmd.Parameters.Add("@PesoMaximo", SqlDbType.Int).Value = tabla.PesoMaximo;
                        cmd.Parameters.Add("@ExperienciaId", SqlDbType.TinyInt).Value = (byte)tabla.Experiencia;
                        cmd.Parameters.Add("@ImagenAtrasUrl", SqlDbType.NVarChar, 500).Value =
                            (object?)tabla.ImagenAtrasUrl ?? DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return tabla.Id;
                }
            }
        }

        public bool EliminarTabla(int id, int shaperId)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        string sqlVerificar = @"
                    SELECT COUNT(*)
                    FROM Productos
                    WHERE Id = @Id
                      AND ShaperId = @ShaperId
                      AND TipoProducto = 'Tabla'";

                        using (SqlCommand comandoVerificar =
                            new SqlCommand(sqlVerificar, conexion, transaccion))
                        {
                            comandoVerificar.Parameters
                                .Add("@Id", SqlDbType.Int)
                                .Value = id;

                            comandoVerificar.Parameters
                                .Add("@ShaperId", SqlDbType.Int)
                                .Value = shaperId;

                            int existe =
                                (int)comandoVerificar.ExecuteScalar();

                            if (existe == 0)
                            {
                                transaccion.Rollback();
                                return false;
                            }
                        }

                        string sqlEliminarTabla = @"
                    DELETE FROM Tablas
                    WHERE ProductoId = @Id";

                        using (SqlCommand comandoTabla =
                            new SqlCommand(
                                sqlEliminarTabla,
                                conexion,
                                transaccion
                            ))
                        {
                            comandoTabla.Parameters
                                .Add("@Id", SqlDbType.Int)
                                .Value = id;

                            comandoTabla.ExecuteNonQuery();
                        }

                        string sqlEliminarProducto = @"
                    DELETE FROM Productos
                    WHERE Id = @Id
                      AND ShaperId = @ShaperId";

                        int filasAfectadas;

                        using (SqlCommand comandoProducto =
                            new SqlCommand(
                                sqlEliminarProducto,
                                conexion,
                                transaccion
                            ))
                        {
                            comandoProducto.Parameters
                                .Add("@Id", SqlDbType.Int)
                                .Value = id;

                            comandoProducto.Parameters
                                .Add("@ShaperId", SqlDbType.Int)
                                .Value = shaperId;

                            filasAfectadas =
                                comandoProducto.ExecuteNonQuery();
                        }

                        transaccion.Commit();

                        return filasAfectadas == 1;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Inserta un Traje siguiendo el patrón TPT.
        /// </summary>
        public int InsertarTraje(Traje traje)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    int idGenerado;

                    string sqlBase = @"INSERT INTO Productos
                                            (Titulo, Subtitulo, Precio, Descripcion, ImagenUrl, ShaperId, TipoProducto)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Titulo, @Subtitulo, @Precio, @Descripcion, @ImagenUrl, @ShaperId, 'Traje')";

                    using (SqlCommand comandoBase = new SqlCommand(sqlBase, conexion, transaccion))
                    {
                        comandoBase.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = traje.Titulo;
                        comandoBase.Parameters.Add("@Subtitulo", SqlDbType.NVarChar, 200).Value = traje.Subtitulo ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@Precio", SqlDbType.Decimal).Value = (decimal)traje.Precio;
                        comandoBase.Parameters.Add("@Descripcion", SqlDbType.NVarChar).Value = traje.Descripcion ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ImagenUrl", SqlDbType.NVarChar, 500).Value = traje.ImagenUrl ?? (object)DBNull.Value;
                        comandoBase.Parameters.Add("@ShaperId", SqlDbType.Int).Value = traje.ShaperId;

                        idGenerado = (int)comandoBase.ExecuteScalar();
                    }

                    string sqlHija = @"INSERT INTO Trajes (ProductoId, Genero, Espesor, Talle, Temperatura)
                                        VALUES (@ProductoId, @Genero, @Espesor, @Talle, @Temperatura)";

                    using (SqlCommand comandoHija = new SqlCommand(sqlHija, conexion, transaccion))
                    {
                        comandoHija.Parameters.Add("@ProductoId", SqlDbType.Int).Value = idGenerado;
                        comandoHija.Parameters.Add("@Genero", SqlDbType.TinyInt).Value = (byte)traje.Genero;
                        comandoHija.Parameters.Add("@Espesor", SqlDbType.Int).Value = traje.Espesor;
                        comandoHija.Parameters.Add("@Talle", SqlDbType.TinyInt).Value = (byte)traje.Talle;
                        comandoHija.Parameters.Add("@Temperatura", SqlDbType.NVarChar, 50).Value = traje.Temperatura ?? (object)DBNull.Value;

                        comandoHija.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return idGenerado;
                }
            }
        }

        public bool ReservarTabla(int productoId, SqlConnection conexion, SqlTransaction transaccion)
        {
            string sql = @"
        UPDATE Tablas WITH (UPDLOCK, ROWLOCK)
        SET Disponible = 0
        WHERE ProductoId = @ProductoId AND Disponible = 1";

            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                int filasAfectadas = comando.ExecuteNonQuery();
                return filasAfectadas == 1; // si ya estaba en 0, no afecta ninguna fila → false
            }
        }

        public bool DescontarStock(int productoId, int cantidad, string tipoProducto, SqlConnection conexion, SqlTransaction transaccion)
        {
            string nombreTabla = ObtenerTablaStock(tipoProducto);

            string sql = $@"
        UPDATE {nombreTabla} WITH (UPDLOCK, ROWLOCK)
        SET Stock = Stock - @Cantidad
        WHERE ProductoId = @ProductoId AND Stock >= @Cantidad";

            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad;

                int filasAfectadas = comando.ExecuteNonQuery();
                return filasAfectadas == 1; // 0 filas = no había stock suficiente
            }
        }

        private string ObtenerTablaStock(string tipoProducto)
        {
            switch (tipoProducto)
            {
                case "Leash": return "Leashes";
                case "Pad": return "Pads";
                case "Quilla": return "Quillas";
                case "Traje": return "Trajes";
                default:
                    throw new ArgumentException($"'{tipoProducto}' no maneja stock (¿es una Tabla? usá ReservarTabla).");
            }
        }
    }
}
