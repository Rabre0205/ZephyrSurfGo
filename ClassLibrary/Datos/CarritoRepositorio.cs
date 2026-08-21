using ClassLibrary.Carrito;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ClassLibrary.Datos
{
    public interface ICarritoRepositorio
    {
        List<CarritoItemDetallado> ObtenerPorUsuario(int usuarioId);
        void AgregarItem(int usuarioId, int productoId, int cantidad);
        bool ActualizarCantidad(int usuarioId, int productoId, int cantidad);
        (string TipoProducto, int? Stock, bool Disponible)? ObtenerDisponibilidad(int productoId);
        void EliminarItem(int usuarioId, int productoId);
        void EliminarItem(int usuarioId, int productoId, SqlConnection conexion, SqlTransaction transaccion); // la que ya usa el checkout
    }

    public class CarritoRepositorio : ICarritoRepositorio
    {
        public List<CarritoItemDetallado> ObtenerPorUsuario(int usuarioId)
        {
            var items = new List<CarritoItemDetallado>();

            string sql = @"
            SELECT ci.ProductoId, p.TipoProducto, p.Titulo, p.Precio, p.ShaperId,
                   ci.Cantidad, p.ImagenUrl,
                   CASE p.TipoProducto
                       WHEN 'Leash' THEN l.Stock
                       WHEN 'Pad' THEN pa.Stock
                       WHEN 'Quilla' THEN q.Stock
                       WHEN 'Traje' THEN tr.Stock
                       ELSE NULL
                   END AS StockDisponible,
                   CASE
                       WHEN p.TipoProducto = 'Tabla' AND t.Disponible = 1 THEN CAST(1 AS BIT)
                       WHEN p.TipoProducto = 'Leash' AND l.Stock >= ci.Cantidad THEN CAST(1 AS BIT)
                       WHEN p.TipoProducto = 'Pad' AND pa.Stock >= ci.Cantidad THEN CAST(1 AS BIT)
                       WHEN p.TipoProducto = 'Quilla' AND q.Stock >= ci.Cantidad THEN CAST(1 AS BIT)
                       WHEN p.TipoProducto = 'Traje' AND tr.Stock >= ci.Cantidad THEN CAST(1 AS BIT)
                       ELSE CAST(0 AS BIT)
                   END AS Disponible
            FROM CarritoItems ci
            INNER JOIN Productos p ON p.Id = ci.ProductoId
            LEFT JOIN Leashes l ON l.ProductoId = p.Id
            LEFT JOIN Pads pa ON pa.ProductoId = p.Id
            LEFT JOIN Quillas q ON q.ProductoId = p.Id
            LEFT JOIN Trajes tr ON tr.ProductoId = p.Id
            LEFT JOIN Tablas t ON t.ProductoId = p.Id
            WHERE ci.UsuarioId = @UsuarioId AND p.DELETED = 0";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        items.Add(new CarritoItemDetallado
                        {
                            ProductoId = lector.GetInt32(0),
                            TipoProducto = lector.GetString(1),
                            Titulo = lector.GetString(2),
                            Precio = (double)lector.GetDecimal(3),
                            ShaperId = lector.GetInt32(4),
                            Cantidad = lector.GetInt32(5),
                            ImagenUrl = lector.IsDBNull(6) ? string.Empty : lector.GetString(6),
                            StockDisponible = lector.IsDBNull(7) ? null : lector.GetInt32(7),
                            Disponible = lector.GetBoolean(8)
                        });
                    }
                }
            }

            return items;
        }

        public void EliminarItem(int usuarioId, int productoId, SqlConnection conexion, SqlTransaction transaccion)
        {
            string sql = "DELETE FROM CarritoItems WHERE UsuarioId = @UsuarioId AND ProductoId = @ProductoId";

            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                comando.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                comando.ExecuteNonQuery();
            }
        }

        public void AgregarItem(int usuarioId, int productoId, int cantidad)
        {
            string sql = @"
        MERGE CarritoItems AS destino
        USING (SELECT @UsuarioId AS UsuarioId, @ProductoId AS ProductoId) AS origen
        ON destino.UsuarioId = origen.UsuarioId AND destino.ProductoId = origen.ProductoId
        WHEN MATCHED THEN UPDATE SET Cantidad =
            CASE
                WHEN EXISTS (
                    SELECT 1 FROM Productos
                    WHERE Id = @ProductoId AND TipoProducto = 'Tabla'
                ) THEN 1
                ELSE destino.Cantidad + @Cantidad
            END
        WHEN NOT MATCHED THEN INSERT (UsuarioId, ProductoId, Cantidad)
            VALUES (@UsuarioId, @ProductoId, @Cantidad);";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                comando.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad;
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public bool ActualizarCantidad(int usuarioId, int productoId, int cantidad)
        {
            const string sql = @"
                UPDATE ci
                SET Cantidad = @Cantidad
                FROM CarritoItems ci
                INNER JOIN Productos p ON p.Id = ci.ProductoId
                LEFT JOIN Leashes l ON l.ProductoId = p.Id
                LEFT JOIN Pads pa ON pa.ProductoId = p.Id
                LEFT JOIN Quillas q ON q.ProductoId = p.Id
                LEFT JOIN Trajes tr ON tr.ProductoId = p.Id
                WHERE ci.UsuarioId = @UsuarioId
                  AND ci.ProductoId = @ProductoId
                  AND @Cantidad >= 1
                  AND ((p.TipoProducto = 'Tabla' AND @Cantidad = 1)
                    OR (p.TipoProducto = 'Leash' AND l.Stock >= @Cantidad)
                    OR (p.TipoProducto = 'Pad' AND pa.Stock >= @Cantidad)
                    OR (p.TipoProducto = 'Quilla' AND q.Stock >= @Cantidad)
                    OR (p.TipoProducto = 'Traje' AND tr.Stock >= @Cantidad));";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                comando.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad;
                conexion.Open();
                return comando.ExecuteNonQuery() == 1;
            }
        }

        public (string TipoProducto, int? Stock, bool Disponible)? ObtenerDisponibilidad(int productoId)
        {
            const string sql = @"
                SELECT p.TipoProducto,
                       CASE p.TipoProducto
                           WHEN 'Leash' THEN l.Stock WHEN 'Pad' THEN pa.Stock
                           WHEN 'Quilla' THEN q.Stock WHEN 'Traje' THEN tr.Stock
                           ELSE NULL END AS Stock,
                       CASE
                           WHEN p.TipoProducto = 'Tabla' THEN t.Disponible
                           WHEN p.TipoProducto = 'Leash' AND l.Stock > 0 THEN CAST(1 AS BIT)
                           WHEN p.TipoProducto = 'Pad' AND pa.Stock > 0 THEN CAST(1 AS BIT)
                           WHEN p.TipoProducto = 'Quilla' AND q.Stock > 0 THEN CAST(1 AS BIT)
                           WHEN p.TipoProducto = 'Traje' AND tr.Stock > 0 THEN CAST(1 AS BIT)
                           ELSE CAST(0 AS BIT) END AS Disponible
                FROM Productos p
                LEFT JOIN Leashes l ON l.ProductoId = p.Id
                LEFT JOIN Pads pa ON pa.ProductoId = p.Id
                LEFT JOIN Quillas q ON q.ProductoId = p.Id
                LEFT JOIN Trajes tr ON tr.ProductoId = p.Id
                LEFT JOIN Tablas t ON t.ProductoId = p.Id
                WHERE p.Id = @ProductoId AND p.DELETED = 0;";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@ProductoId", SqlDbType.Int).Value = productoId;
                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (!lector.Read()) return null;
                    return (
                        lector.GetString(0),
                        lector.IsDBNull(1) ? null : lector.GetInt32(1),
                        lector.GetBoolean(2));
                }
            }
        }

        // Sobrecarga para uso "suelto" (fuera de una transacción de checkout)
        public void EliminarItem(int usuarioId, int productoId)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    EliminarItem(usuarioId, productoId, conexion, transaccion);
                    transaccion.Commit();
                }
            }
        }
    }
}
