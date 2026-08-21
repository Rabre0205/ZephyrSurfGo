using ClassLibrary.Pedidos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos
{
    public interface IPedidoRepositorio
    {
        int ContarPedidos();
        int ContarPedidos(string busqueda, byte? estadoId);

        int ContarPedidosPorEstado(byte estadoId);

        List<PedidoAdminItem> ObtenerPedidosAdministracion(
            int pagina,
            int cantidadPorPagina
        );
        List<PedidoAdminItem> ObtenerPedidosAdministracion(
            string busqueda, byte? estadoId, int pagina, int cantidadPorPagina);
        PedidoAdminDetalle ObtenerDetalleAdministracion(int pedidoId);
        int ContarPedidosShaper(int shaperId, string busqueda, byte? estadoId);
        List<PedidoAdminItem> ObtenerPedidosShaper(
            int shaperId, string busqueda, byte? estadoId,
            int pagina, int cantidadPorPagina);
        PedidoAdminDetalle ObtenerDetalleShaper(int pedidoId, int shaperId);
        List<PedidoAdminItem> ObtenerPedidosCliente(int clienteId);
        PedidoAdminDetalle? ObtenerDetalleCliente(int pedidoId, int clienteId);
        (int TotalPedidos, int PedidosPendientes, decimal VentasConfirmadas,
         decimal Comisiones) ObtenerResumenShaper(int shaperId);

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

        (
            int TotalPedidos,
            decimal VentasTotales,
            decimal ComisionTotal
        ) ObtenerResumenAdministracion();
    }

    public class PedidoRepositorio : IPedidoRepositorio
    {
        public int ContarPedidos()
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Pedidos;
            ";

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
            {
                conexion.Open();

                return Convert.ToInt32(
                    comando.ExecuteScalar()
                );
            }
        }

        public int ContarPedidos(string busqueda, byte? estadoId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Pedidos p
                INNER JOIN Usuarios c ON c.Id = p.ClienteId
                INNER JOIN Usuarios s ON s.Id = p.ShaperId
                WHERE (@Busqueda = '' OR c.Nombre LIKE '%' + @Busqueda + '%'
                       OR c.Email LIKE '%' + @Busqueda + '%'
                       OR s.Nombre LIKE '%' + @Busqueda + '%'
                       OR s.NombreDeNegosio LIKE '%' + @Busqueda + '%'
                       OR CONVERT(NVARCHAR(20), p.Id) = @Busqueda)
                  AND (@EstadoId IS NULL OR p.EstadoPedidoId = @EstadoId);";
            using var conexion = Conexion.ObtenerConexion();
            using var comando = new SqlCommand(sql, conexion);
            AgregarFiltrosPedidos(comando, busqueda, estadoId);
            conexion.Open();
            return Convert.ToInt32(comando.ExecuteScalar());
        }

        public int ContarPedidosPorEstado(byte estadoId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Pedidos
                WHERE EstadoPedidoId = @EstadoId;
            ";

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
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

        public List<PedidoAdminItem> ObtenerPedidosAdministracion(
            int pagina,
            int cantidadPorPagina)
        {
            var pedidos = new List<PedidoAdminItem>();

            const string sql = @"
                SELECT
                    p.Id,
                    p.EstadoPedidoId,
                    estado.Nombre AS EstadoNombre,
                    p.Total,
                    p.ComisionPlataforma,
                    p.FechaCreacion,

                    cliente.Nombre AS ClienteNombre,
                    cliente.Email AS ClienteEmail,

                    shaper.Nombre AS ShaperNombre,
                    shaper.NombreDeNegosio AS NegocioShaper

                FROM Pedidos p

                INNER JOIN Usuarios cliente
                    ON cliente.Id = p.ClienteId

                INNER JOIN Usuarios shaper
                    ON shaper.Id = p.ShaperId

                INNER JOIN EstadosPedido estado
                    ON estado.Id = p.EstadoPedidoId

                ORDER BY p.FechaCreacion DESC, p.Id DESC

                OFFSET @Desplazamiento ROWS
                FETCH NEXT @Cantidad ROWS ONLY;
            ";

            int desplazamiento =
                (pagina - 1) * cantidadPorPagina;

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@Desplazamiento",
                    SqlDbType.Int
                ).Value = desplazamiento;

                comando.Parameters.Add(
                    "@Cantidad",
                    SqlDbType.Int
                ).Value = cantidadPorPagina;

                conexion.Open();

                using (SqlDataReader lector =
                    comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        var pedido = new PedidoAdminItem
                        {
                            Id = Convert.ToInt32(
                                lector["Id"]
                            ),

                            ClienteNombre =
                                Convert.ToString(
                                    lector["ClienteNombre"]
                                ) ?? string.Empty,

                            ClienteEmail =
                                Convert.ToString(
                                    lector["ClienteEmail"]
                                ) ?? string.Empty,

                            ShaperNombre =
                                Convert.ToString(
                                    lector["ShaperNombre"]
                                ) ?? string.Empty,

                            NegocioShaper =
                                lector["NegocioShaper"] == DBNull.Value
                                    ? string.Empty
                                    : Convert.ToString(
                                        lector["NegocioShaper"]
                                      ) ?? string.Empty,

                            EstadoId = Convert.ToByte(
                                lector["EstadoPedidoId"]
                            ),

                            EstadoNombre =
                                Convert.ToString(
                                    lector["EstadoNombre"]
                                ) ?? string.Empty,

                            Total = Convert.ToDecimal(
                                lector["Total"]
                            ),

                            ComisionPlataforma =
                                Convert.ToDecimal(
                                    lector["ComisionPlataforma"]
                                ),

                            FechaCreacion =
                                Convert.ToDateTime(
                                    lector["FechaCreacion"]
                                )
                        };

                        pedidos.Add(pedido);
                    }
                }
            }

            return pedidos;
        }

        public List<PedidoAdminItem> ObtenerPedidosAdministracion(
            string busqueda, byte? estadoId, int pagina, int cantidadPorPagina)
        {
            var pedidos = new List<PedidoAdminItem>();
            const string sql = @"
                SELECT p.Id, p.EstadoPedidoId, e.Nombre EstadoNombre, p.Total,
                       p.ComisionPlataforma, p.FechaCreacion,
                       c.Nombre ClienteNombre, c.Email ClienteEmail,
                       s.Nombre ShaperNombre, s.NombreDeNegosio NegocioShaper
                FROM Pedidos p
                INNER JOIN Usuarios c ON c.Id = p.ClienteId
                INNER JOIN Usuarios s ON s.Id = p.ShaperId
                INNER JOIN EstadosPedido e ON e.Id = p.EstadoPedidoId
                WHERE (@Busqueda = '' OR c.Nombre LIKE '%' + @Busqueda + '%'
                       OR c.Email LIKE '%' + @Busqueda + '%'
                       OR s.Nombre LIKE '%' + @Busqueda + '%'
                       OR s.NombreDeNegosio LIKE '%' + @Busqueda + '%'
                       OR CONVERT(NVARCHAR(20), p.Id) = @Busqueda)
                  AND (@EstadoId IS NULL OR p.EstadoPedidoId = @EstadoId)
                ORDER BY p.FechaCreacion DESC, p.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @Cantidad ROWS ONLY;";
            using var conexion = Conexion.ObtenerConexion();
            using var comando = new SqlCommand(sql, conexion);
            AgregarFiltrosPedidos(comando, busqueda, estadoId);
            comando.Parameters.Add("@Offset", SqlDbType.Int).Value = (pagina - 1) * cantidadPorPagina;
            comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidadPorPagina;
            conexion.Open();
            using var lector = comando.ExecuteReader();
            while (lector.Read()) pedidos.Add(MapearPedidoAdmin(lector));
            return pedidos;
        }

        public PedidoAdminDetalle ObtenerDetalleAdministracion(int pedidoId)
        {
            const string cabecera = @"
                SELECT p.Id,p.EstadoPedidoId,e.Nombre EstadoNombre,p.Total,p.ComisionPlataforma,
                       p.FechaCreacion,p.MercadoPagoPreferenceId,p.MercadoPagoPaymentId,
                       c.Nombre ClienteNombre,c.Email ClienteEmail,
                       s.Nombre ShaperNombre,s.NombreDeNegosio NegocioShaper
                FROM Pedidos p INNER JOIN Usuarios c ON c.Id=p.ClienteId
                INNER JOIN Usuarios s ON s.Id=p.ShaperId
                INNER JOIN EstadosPedido e ON e.Id=p.EstadoPedidoId WHERE p.Id=@Id;";
            using var conexion = Conexion.ObtenerConexion();
            conexion.Open();
            PedidoAdminDetalle detalle;
            using (var comando = new SqlCommand(cabecera, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = pedidoId;
                using var lector = comando.ExecuteReader();
                if (!lector.Read()) return null;
                var baseItem = MapearPedidoAdmin(lector);
                detalle = new PedidoAdminDetalle
                {
                    Id=baseItem.Id, ClienteNombre=baseItem.ClienteNombre, ClienteEmail=baseItem.ClienteEmail,
                    ShaperNombre=baseItem.ShaperNombre, NegocioShaper=baseItem.NegocioShaper,
                    EstadoId=baseItem.EstadoId, EstadoNombre=baseItem.EstadoNombre, Total=baseItem.Total,
                    ComisionPlataforma=baseItem.ComisionPlataforma, FechaCreacion=baseItem.FechaCreacion,
                    MercadoPagoPreferenceId=lector["MercadoPagoPreferenceId"]==DBNull.Value?string.Empty:Convert.ToString(lector["MercadoPagoPreferenceId"])??string.Empty,
                    MercadoPagoPaymentId=lector["MercadoPagoPaymentId"]==DBNull.Value?string.Empty:Convert.ToString(lector["MercadoPagoPaymentId"])??string.Empty
                };
            }
            const string itemsSql = "SELECT ProductoId,TituloSnapshot,PrecioUnitarioSnapshot,Cantidad FROM PedidoItems WHERE PedidoId=@Id ORDER BY Id;";
            using (var comando = new SqlCommand(itemsSql, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = pedidoId;
                using var lector = comando.ExecuteReader();
                while (lector.Read()) detalle.Items.Add(new PedidoItem
                {
                    ProductoId=Convert.ToInt32(lector["ProductoId"]),
                    TituloSnapshot=Convert.ToString(lector["TituloSnapshot"])??string.Empty,
                    PrecioUnitarioSnapshot=(double)Convert.ToDecimal(lector["PrecioUnitarioSnapshot"]),
                    Cantidad=Convert.ToInt32(lector["Cantidad"])
                });
            }
            return detalle;
        }

        public int ContarPedidosShaper(int shaperId, string busqueda, byte? estadoId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Pedidos p
                INNER JOIN Usuarios c ON c.Id = p.ClienteId
                WHERE p.ShaperId = @ShaperId
                  AND (@Busqueda = '' OR c.Nombre LIKE '%' + @Busqueda + '%'
                       OR c.Email LIKE '%' + @Busqueda + '%'
                       OR CONVERT(NVARCHAR(20), p.Id) = @Busqueda)
                  AND (@EstadoId IS NULL OR p.EstadoPedidoId = @EstadoId);";
            using var conexion = Conexion.ObtenerConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
            AgregarFiltrosPedidos(comando, busqueda, estadoId);
            conexion.Open();
            return Convert.ToInt32(comando.ExecuteScalar());
        }

        public List<PedidoAdminItem> ObtenerPedidosShaper(
            int shaperId, string busqueda, byte? estadoId,
            int pagina, int cantidadPorPagina)
        {
            var pedidos = new List<PedidoAdminItem>();
            const string sql = @"
                SELECT p.Id,p.EstadoPedidoId,e.Nombre EstadoNombre,p.Total,
                       p.ComisionPlataforma,p.FechaCreacion,
                       c.Nombre ClienteNombre,c.Email ClienteEmail,
                       s.Nombre ShaperNombre,s.NombreDeNegosio NegocioShaper
                FROM Pedidos p
                INNER JOIN Usuarios c ON c.Id=p.ClienteId
                INNER JOIN Usuarios s ON s.Id=p.ShaperId
                INNER JOIN EstadosPedido e ON e.Id=p.EstadoPedidoId
                WHERE p.ShaperId=@ShaperId
                  AND (@Busqueda='' OR c.Nombre LIKE '%' + @Busqueda + '%'
                       OR c.Email LIKE '%' + @Busqueda + '%'
                       OR CONVERT(NVARCHAR(20),p.Id)=@Busqueda)
                  AND (@EstadoId IS NULL OR p.EstadoPedidoId=@EstadoId)
                ORDER BY p.FechaCreacion DESC,p.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @Cantidad ROWS ONLY;";
            using var conexion = Conexion.ObtenerConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
            AgregarFiltrosPedidos(comando, busqueda, estadoId);
            comando.Parameters.Add("@Offset", SqlDbType.Int).Value = (pagina - 1) * cantidadPorPagina;
            comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidadPorPagina;
            conexion.Open();
            using var lector = comando.ExecuteReader();
            while (lector.Read()) pedidos.Add(MapearPedidoAdmin(lector));
            return pedidos;
        }

        public PedidoAdminDetalle ObtenerDetalleShaper(int pedidoId, int shaperId)
        {
            const string cabecera = @"
                SELECT p.Id,p.EstadoPedidoId,e.Nombre EstadoNombre,p.Total,p.ComisionPlataforma,
                       p.FechaCreacion,p.MercadoPagoPreferenceId,p.MercadoPagoPaymentId,
                       c.Nombre ClienteNombre,c.Email ClienteEmail,
                       s.Nombre ShaperNombre,s.NombreDeNegosio NegocioShaper
                FROM Pedidos p INNER JOIN Usuarios c ON c.Id=p.ClienteId
                INNER JOIN Usuarios s ON s.Id=p.ShaperId
                INNER JOIN EstadosPedido e ON e.Id=p.EstadoPedidoId
                WHERE p.Id=@Id AND p.ShaperId=@ShaperId;";
            using var conexion = Conexion.ObtenerConexion();
            conexion.Open();
            PedidoAdminDetalle detalle;
            using (var comando = new SqlCommand(cabecera, conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = pedidoId;
                comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
                using var lector = comando.ExecuteReader();
                if (!lector.Read()) return null;
                var item = MapearPedidoAdmin(lector);
                detalle = new PedidoAdminDetalle
                {
                    Id=item.Id,ClienteNombre=item.ClienteNombre,ClienteEmail=item.ClienteEmail,
                    ShaperNombre=item.ShaperNombre,NegocioShaper=item.NegocioShaper,
                    EstadoId=item.EstadoId,EstadoNombre=item.EstadoNombre,Total=item.Total,
                    ComisionPlataforma=item.ComisionPlataforma,FechaCreacion=item.FechaCreacion,
                    MercadoPagoPreferenceId=lector["MercadoPagoPreferenceId"]==DBNull.Value?string.Empty:Convert.ToString(lector["MercadoPagoPreferenceId"])??string.Empty,
                    MercadoPagoPaymentId=lector["MercadoPagoPaymentId"]==DBNull.Value?string.Empty:Convert.ToString(lector["MercadoPagoPaymentId"])??string.Empty
                };
            }
            using (var comando = new SqlCommand("SELECT ProductoId,TituloSnapshot,PrecioUnitarioSnapshot,Cantidad FROM PedidoItems WHERE PedidoId=@Id ORDER BY Id", conexion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = pedidoId;
                using var lector = comando.ExecuteReader();
                while (lector.Read()) detalle.Items.Add(new PedidoItem
                {
                    ProductoId=Convert.ToInt32(lector["ProductoId"]),
                    TituloSnapshot=Convert.ToString(lector["TituloSnapshot"])??string.Empty,
                    PrecioUnitarioSnapshot=(double)Convert.ToDecimal(lector["PrecioUnitarioSnapshot"]),
                    Cantidad=Convert.ToInt32(lector["Cantidad"])
                });
            }
            return detalle;
        }

        public List<PedidoAdminItem> ObtenerPedidosCliente(int clienteId)
        {
            var pedidos = new List<PedidoAdminItem>();
            const string sql = @"
                SELECT p.Id,p.EstadoPedidoId,e.Nombre EstadoNombre,p.Total,
                       p.ComisionPlataforma,p.FechaCreacion,
                       c.Nombre ClienteNombre,c.Email ClienteEmail,
                       s.Nombre ShaperNombre,s.NombreDeNegosio NegocioShaper
                FROM Pedidos p
                INNER JOIN Usuarios c ON c.Id=p.ClienteId
                INNER JOIN Usuarios s ON s.Id=p.ShaperId
                INNER JOIN EstadosPedido e ON e.Id=p.EstadoPedidoId
                WHERE p.ClienteId=@ClienteId
                ORDER BY p.FechaCreacion DESC,p.Id DESC;";
            using var conexion = Conexion.ObtenerConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            conexion.Open();
            using var lector = comando.ExecuteReader();
            while (lector.Read()) pedidos.Add(MapearPedidoAdmin(lector));
            return pedidos;
        }

        public PedidoAdminDetalle? ObtenerDetalleCliente(int pedidoId, int clienteId)
        {
            const string sql = "SELECT COUNT(*) FROM Pedidos WHERE Id=@Id AND ClienteId=@ClienteId";
            using var conexion = Conexion.ObtenerConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@Id", SqlDbType.Int).Value = pedidoId;
            comando.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            conexion.Open();
            return Convert.ToInt32(comando.ExecuteScalar()) == 1
                ? ObtenerDetalleAdministracion(pedidoId)
                : null;
        }

        public (int TotalPedidos, int PedidosPendientes, decimal VentasConfirmadas,
                decimal Comisiones) ObtenerResumenShaper(int shaperId)
        {
            const string sql = @"
                SELECT COUNT(*) TotalPedidos,
                       COALESCE(SUM(CASE WHEN EstadoPedidoId=0 THEN 1 ELSE 0 END),0) PedidosPendientes,
                       COALESCE(SUM(CASE WHEN EstadoPedidoId IN (1,4) THEN Total ELSE 0 END),0) VentasConfirmadas,
                       COALESCE(SUM(CASE WHEN EstadoPedidoId IN (1,4) THEN ComisionPlataforma ELSE 0 END),0) Comisiones
                FROM Pedidos WHERE ShaperId=@ShaperId;";
            using var conexion = Conexion.ObtenerConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@ShaperId", SqlDbType.Int).Value = shaperId;
            conexion.Open();
            using var lector = comando.ExecuteReader();
            if (!lector.Read()) return (0, 0, 0, 0);
            return (Convert.ToInt32(lector["TotalPedidos"]),
                    Convert.ToInt32(lector["PedidosPendientes"]),
                    Convert.ToDecimal(lector["VentasConfirmadas"]),
                    Convert.ToDecimal(lector["Comisiones"]));
        }

        private static void AgregarFiltrosPedidos(SqlCommand comando, string busqueda, byte? estadoId)
        {
            comando.Parameters.Add("@Busqueda", SqlDbType.NVarChar, 150).Value = busqueda ?? string.Empty;
            comando.Parameters.Add("@EstadoId", SqlDbType.TinyInt).Value =
                estadoId.HasValue ? (object)estadoId.Value : DBNull.Value;
        }

        private static PedidoAdminItem MapearPedidoAdmin(SqlDataReader lector) => new()
        {
            Id=Convert.ToInt32(lector["Id"]), ClienteNombre=Convert.ToString(lector["ClienteNombre"])??string.Empty,
            ClienteEmail=Convert.ToString(lector["ClienteEmail"])??string.Empty,
            ShaperNombre=Convert.ToString(lector["ShaperNombre"])??string.Empty,
            NegocioShaper=lector["NegocioShaper"]==DBNull.Value?string.Empty:Convert.ToString(lector["NegocioShaper"])??string.Empty,
            EstadoId=Convert.ToByte(lector["EstadoPedidoId"]), EstadoNombre=Convert.ToString(lector["EstadoNombre"])??string.Empty,
            Total=Convert.ToDecimal(lector["Total"]), ComisionPlataforma=Convert.ToDecimal(lector["ComisionPlataforma"]),
            FechaCreacion=Convert.ToDateTime(lector["FechaCreacion"])
        };

        public int Insertar(
            Pedido pedido,
            SqlConnection conexion,
            SqlTransaction transaccion)
        {
            const string sqlCabecera = @"
                DECLARE @Insertados TABLE (Id INT);

                INSERT INTO Pedidos
                (
                    ClienteId,
                    ShaperId,
                    EstadoPedidoId,
                    Total,
                    ComisionPlataforma
                )
                OUTPUT INSERTED.Id INTO @Insertados
                VALUES
                (
                    @ClienteId,
                    @ShaperId,
                    @EstadoPedidoId,
                    @Total,
                    @ComisionPlataforma
                );

                SELECT Id FROM @Insertados;
            ";

            int pedidoId;

            using (SqlCommand comando =
                new SqlCommand(
                    sqlCabecera,
                    conexion,
                    transaccion
                ))
            {
                comando.Parameters.Add(
                    "@ClienteId",
                    SqlDbType.Int
                ).Value = pedido.ClienteId;

                comando.Parameters.Add(
                    "@ShaperId",
                    SqlDbType.Int
                ).Value = pedido.ShaperId;

                comando.Parameters.Add(
                    "@EstadoPedidoId",
                    SqlDbType.TinyInt
                ).Value = pedido.EstadoPedidoId;

                comando.Parameters.Add(
                    "@Total",
                    SqlDbType.Decimal
                ).Value = (decimal)pedido.Total;

                comando.Parameters.Add(
                    "@ComisionPlataforma",
                    SqlDbType.Decimal
                ).Value = (decimal)pedido.ComisionPlataforma;

                pedidoId = (int)comando.ExecuteScalar();
            }

            const string sqlItem = @"
                INSERT INTO PedidoItems
                (
                    PedidoId,
                    ProductoId,
                    TituloSnapshot,
                    PrecioUnitarioSnapshot,
                    Cantidad
                )
                VALUES
                (
                    @PedidoId,
                    @ProductoId,
                    @Titulo,
                    @Precio,
                    @Cantidad
                );
            ";

            foreach (PedidoItem item in pedido.Items)
            {
                using (SqlCommand comandoItem =
                    new SqlCommand(
                        sqlItem,
                        conexion,
                        transaccion
                    ))
                {
                    comandoItem.Parameters.Add(
                        "@PedidoId",
                        SqlDbType.Int
                    ).Value = pedidoId;

                    comandoItem.Parameters.Add(
                        "@ProductoId",
                        SqlDbType.Int
                    ).Value = item.ProductoId;

                    comandoItem.Parameters.Add(
                        "@Titulo",
                        SqlDbType.NVarChar,
                        150
                    ).Value = item.TituloSnapshot;

                    comandoItem.Parameters.Add(
                        "@Precio",
                        SqlDbType.Decimal
                    ).Value =
                        (decimal)item.PrecioUnitarioSnapshot;

                    comandoItem.Parameters.Add(
                        "@Cantidad",
                        SqlDbType.Int
                    ).Value = item.Cantidad;

                    comandoItem.ExecuteNonQuery();
                }
            }

            return pedidoId;
        }

        public void GuardarPreferenceId(
            int pedidoId,
            string preferenceId)
        {
            const string sql = @"
                UPDATE Pedidos
                SET MercadoPagoPreferenceId = @PreferenceId
                WHERE Id = @PedidoId;
            ";

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@PreferenceId",
                    SqlDbType.NVarChar,
                    100
                ).Value = preferenceId;

                comando.Parameters.Add(
                    "@PedidoId",
                    SqlDbType.Int
                ).Value = pedidoId;

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public void ActualizarEstado(
            int pedidoId,
            byte nuevoEstadoId,
            string mercadoPagoPaymentId)
        {
            const string sql = @"
                UPDATE Pedidos
                SET
                    EstadoPedidoId = @EstadoId,
                    MercadoPagoPaymentId = @PaymentId,
                    FechaActualizacion = SYSUTCDATETIME()
                WHERE Id = @PedidoId;
            ";

            using (SqlConnection conexion =
                Conexion.ObtenerConexion())
            using (SqlCommand comando =
                new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add(
                    "@EstadoId",
                    SqlDbType.TinyInt
                ).Value = nuevoEstadoId;

                comando.Parameters.Add(
                    "@PaymentId",
                    SqlDbType.NVarChar,
                    100
                ).Value =
                    (object)mercadoPagoPaymentId ??
                    DBNull.Value;

                comando.Parameters.Add(
                    "@PedidoId",
                    SqlDbType.Int
                ).Value = pedidoId;

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public (
            int TotalPedidos,
            decimal VentasTotales,
            decimal ComisionTotal
        ) ObtenerResumenAdministracion()
        {
            const string sql = @"
                SELECT
                    COUNT(*) AS TotalPedidos,
                    COALESCE(SUM(Total), 0) AS VentasTotales,
                    COALESCE(
                        SUM(ComisionPlataforma),
                        0
                    ) AS ComisionTotal
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
            // Este método se implementará cuando hagamos
            // la pantalla de detalle del pedido.
            throw new NotImplementedException();
        }
    }
}
