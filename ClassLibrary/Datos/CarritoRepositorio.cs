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
        void EliminarItem(int usuarioId, int productoId, SqlConnection conexion, SqlTransaction transaccion);
    }

    public class CarritoRepositorio : ICarritoRepositorio
    {
        public List<CarritoItemDetallado> ObtenerPorUsuario(int usuarioId)
        {
            var items = new List<CarritoItemDetallado>();

            string sql = @"
            SELECT ci.ProductoId, p.TipoProducto, p.Titulo, p.Precio, p.ShaperId, ci.Cantidad
            FROM CarritoItems ci
            INNER JOIN Productos p ON p.Id = ci.ProductoId
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
                            Cantidad = lector.GetInt32(5)
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
    }
}
