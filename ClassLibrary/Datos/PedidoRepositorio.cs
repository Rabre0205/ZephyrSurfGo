using ClassLibrary.Pedidos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ClassLibrary.Datos
{
    public interface IPedidoRepositorio
    {

        int ContarPedidosPorEstado(byte estadoId);

        int Insertar(
            Pedido pedido,
            SqlConnection conexion,
            SqlTransaction transaccion
        );

        void GuardarPreferenceId(
            int pedidoId,
            string preferenceId
        );

        void ActualizarEstado(
            int pedidoId,
            byte nuevoEstadoId,
            string mercadoPagoPaymentId
        );

        Pedido ObtenerPorId(int pedidoId);

        (int TotalPedidos,
         decimal VentasTotales,
         decimal ComisionTotal)
        ObtenerResumenAdministracion();
    }

    public class PedidoRepositorio : IPedidoRepositorio
    {
        public int Insertar(Pedido pedido, SqlConnection conexion, SqlTransaction transaccion)
        {
            string sqlCabecera = @"
            DECLARE @Insertados TABLE (Id INT);

            INSERT INTO Pedidos (ClienteId, ShaperId, EstadoPedidoId, Total, ComisionPlataforma)
            OUTPUT INSERTED.Id INTO @Insertados
            VALUES (@ClienteId, @ShaperId, @EstadoPedidoId, @Total, @ComisionPlataforma);

            SELECT Id FROM @Insertados;";

            int pedidoId;
            using (SqlCommand comando = new SqlCommand(sqlCabecera, conexion, transaccion))
            {
                comando.Parameters.Add("@ClienteId", SqlDbType.Int).Value = pedido.ClienteId;
                comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = pedido.ShaperId;
                comando.Parameters.Add("@EstadoPedidoId", SqlDbType.TinyInt).Value = pedido.EstadoPedidoId;
                comando.Parameters.Add("@Total", SqlDbType.Decimal).Value = (decimal)pedido.Total;
                comando.Parameters.Add("@ComisionPlataforma", SqlDbType.Decimal).Value = (decimal)pedido.ComisionPlataforma;

                pedidoId = (int)comando.ExecuteScalar();
            }

            string sqlItem = @"
            INSERT INTO PedidoItems (PedidoId, ProductoId, TituloSnapshot, PrecioUnitarioSnapshot, Cantidad)
            VALUES (@PedidoId, @ProductoId, @Titulo, @Precio, @Cantidad)";

            foreach (PedidoItem item in pedido.Items)
            {
                using (SqlCommand comandoItem = new SqlCommand(sqlItem, conexion, transaccion))
                {
                    comandoItem.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
                    comandoItem.Parameters.Add("@ProductoId", SqlDbType.Int).Value = item.ProductoId;
                    comandoItem.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = item.TituloSnapshot;
                    comandoItem.Parameters.Add("@Precio", SqlDbType.Decimal).Value = (decimal)item.PrecioUnitarioSnapshot;
                    comandoItem.Parameters.Add("@Cantidad", SqlDbType.Int).Value = item.Cantidad;

                    comandoItem.ExecuteNonQuery();
                }
            }

            return pedidoId;
        }

        public int ContarPedidosPorEstado(byte estadoId)
        {
            string sql = @"
        SELECT COUNT(*)
        FROM Pedidos
        WHERE EstadoPedidoId = @EstadoId;
    ";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@EstadoId",
                    SqlDbType.TinyInt
                ).Value = estadoId;

                conexion.Open();

                return Convert.ToInt32(
                    comando.ExecuteScalar()
                );
            }
        }

        public void GuardarPreferenceId(int pedidoId, string preferenceId)
        {
            string sql = "UPDATE Pedidos SET MercadoPagoPreferenceId = @PreferenceId WHERE Id = @PedidoId";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@PreferenceId", SqlDbType.NVarChar, 100).Value = preferenceId;
                comando.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public void ActualizarEstado(int pedidoId, byte nuevoEstadoId, string mercadoPagoPaymentId)
        {
            string sql = @"
            UPDATE Pedidos
            SET EstadoPedidoId = @EstadoId, MercadoPagoPaymentId = @PaymentId, FechaActualizacion = SYSUTCDATETIME()
            WHERE Id = @PedidoId";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@EstadoId", SqlDbType.TinyInt).Value = nuevoEstadoId;
                comando.Parameters.Add("@PaymentId", SqlDbType.NVarChar, 100).Value = (object)mercadoPagoPaymentId ?? DBNull.Value;
                comando.Parameters.Add("@PedidoId", SqlDbType.Int).Value = pedidoId;
                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        //no implementado

        public (
    int TotalPedidos,
    decimal VentasTotales,
    decimal ComisionTotal
) ObtenerResumenAdministracion()
        {
            string sql = @"
        SELECT
            COUNT(*) AS TotalPedidos,
            COALESCE(SUM(Total), 0) AS VentasTotales,
            COALESCE(SUM(ComisionPlataforma), 0) AS ComisionTotal
        FROM Pedidos;
    ";

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
            {
                conexion.Open();

                using (SqlDataReader lector =
                    comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        int totalPedidos =
                            Convert.ToInt32(
                                lector["TotalPedidos"]
                            );

                        decimal ventasTotales =
                            Convert.ToDecimal(
                                lector["VentasTotales"]
                            );

                        decimal comisionTotal =
                            Convert.ToDecimal(
                                lector["ComisionTotal"]
                            );

                        return (
                            totalPedidos,
                            ventasTotales,
                            comisionTotal
                        );
                    }
                }
            }

            return (0, 0, 0);
        }

        public Pedido ObtenerPorId(int pedidoId)
        {
            // SELECT + mapeo análogo al resto de tus repositorios — lo dejo
            // fuera para no alargar, avisame si lo necesitás completo ahora.
            throw new NotImplementedException();
        }
    }
}
