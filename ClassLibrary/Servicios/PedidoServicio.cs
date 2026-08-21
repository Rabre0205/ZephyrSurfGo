using ClassLibrary.Carrito;
using ClassLibrary.Datos;
using ClassLibrary.Pedidos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;

namespace ClassLibrary.Servicios
{
    public interface IPedidoServicio
    {
        Task<List<(Pedido Pedido, string UrlPago)>>
            CrearPedidosDesdeCarritoAsync(int clienteId);

        Task ProcesarNotificacionPagoAsync(
            string mercadoPagoPaymentId
        );

        int ContarPedidos();
        int ContarPedidos(string busqueda, byte? estadoId);

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
        (int TotalPedidos, int PedidosPendientes, decimal VentasConfirmadas,
         decimal Comisiones) ObtenerResumenShaper(int shaperId);

        (
            int TotalPedidos,
            decimal VentasTotales,
            decimal ComisionTotal
        ) ObtenerResumenAdministracion();

        int ContarPedidosPorEstado(byte estadoId);
    }

    public class PedidoServicio : IPedidoServicio
    {
        private readonly ICarritoRepositorio _carritoRepositorio;
        private readonly IProductoRepositorio _productoRepositorio;
        private readonly IPedidoRepositorio _pedidoRepositorio;
        private readonly IMercadoPagoServicio _mercadoPagoServicio;


        private readonly decimal _comision;

        public PedidoServicio(
            ICarritoRepositorio carritoRepositorio,
            IProductoRepositorio productoRepositorio,
            IPedidoRepositorio pedidoRepositorio,
            IMercadoPagoServicio mercadoPagoServicio)
        {
            _carritoRepositorio = carritoRepositorio;
            _productoRepositorio = productoRepositorio;
            _pedidoRepositorio = pedidoRepositorio;
            _mercadoPagoServicio = mercadoPagoServicio;

            string? comisionConfigurada =
                Environment.GetEnvironmentVariable("MP_COMISION_PLATAFORMA");

            if (!decimal.TryParse(
                    comisionConfigurada,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out _comision))
            {
                throw new InvalidOperationException(
                    "MP_COMISION_PLATAFORMA debe contener un número válido.");
            }
        }

        public int ContarPedidosPorEstado(byte estadoId)
        {
            return _pedidoRepositorio
                .ContarPedidosPorEstado(estadoId);
        }

        public int ContarPedidos()
        {
            return _pedidoRepositorio.ContarPedidos();
        }

        public int ContarPedidos(string busqueda, byte? estadoId) =>
            _pedidoRepositorio.ContarPedidos(busqueda, estadoId);

        public List<PedidoAdminItem> ObtenerPedidosAdministracion(
            string busqueda, byte? estadoId, int pagina, int cantidadPorPagina) =>
            _pedidoRepositorio.ObtenerPedidosAdministracion(
                busqueda, estadoId, pagina, cantidadPorPagina);

        public PedidoAdminDetalle ObtenerDetalleAdministracion(int pedidoId) =>
            _pedidoRepositorio.ObtenerDetalleAdministracion(pedidoId);

        public int ContarPedidosShaper(int shaperId, string busqueda, byte? estadoId) =>
            _pedidoRepositorio.ContarPedidosShaper(shaperId, busqueda, estadoId);

        public List<PedidoAdminItem> ObtenerPedidosShaper(
            int shaperId, string busqueda, byte? estadoId,
            int pagina, int cantidadPorPagina) =>
            _pedidoRepositorio.ObtenerPedidosShaper(
                shaperId, busqueda, estadoId, pagina, cantidadPorPagina);

        public PedidoAdminDetalle ObtenerDetalleShaper(int pedidoId, int shaperId) =>
            _pedidoRepositorio.ObtenerDetalleShaper(pedidoId, shaperId);

        public (int TotalPedidos, int PedidosPendientes, decimal VentasConfirmadas,
                decimal Comisiones) ObtenerResumenShaper(int shaperId) =>
            _pedidoRepositorio.ObtenerResumenShaper(shaperId);

        public List<PedidoAdminItem> ObtenerPedidosAdministracion(
            int pagina,
            int cantidadPorPagina)
        {
            return _pedidoRepositorio
                .ObtenerPedidosAdministracion(
                    pagina,
                    cantidadPorPagina
                );
        }

        public (
    int TotalPedidos,
    decimal VentasTotales,
    decimal ComisionTotal
) ObtenerResumenAdministracion()
        {
            return _pedidoRepositorio
                .ObtenerResumenAdministracion();
        }

        public async Task<List<(Pedido, string)>> CrearPedidosDesdeCarritoAsync(int clienteId)
        {
            List<CarritoItemDetallado> items = _carritoRepositorio.ObtenerPorUsuario(clienteId);
            if (items.Count == 0)
                throw new InvalidOperationException("El carrito está vacío.");

            // Agrupar por shaper, sin LINQ
            var itemsPorShaper = new Dictionary<int, List<CarritoItemDetallado>>();
            foreach (CarritoItemDetallado item in items)
            {
                if (!itemsPorShaper.ContainsKey(item.ShaperId))
                    itemsPorShaper[item.ShaperId] = new List<CarritoItemDetallado>();
                itemsPorShaper[item.ShaperId].Add(item);
            }

            var resultado = new List<(Pedido, string)>();

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                conexion.Open();

                foreach (KeyValuePair<int, List<CarritoItemDetallado>> grupo in itemsPorShaper)
                {
                    int shaperId = grupo.Key;
                    List<CarritoItemDetallado> itemsShaper = grupo.Value;

                    Pedido pedido = new Pedido { ClienteId = clienteId, ShaperId = shaperId, EstadoPedidoId = 0 };

                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            double total = 0;

                            foreach (CarritoItemDetallado item in itemsShaper)
                            {
                                bool reservado = item.TipoProducto == "Tabla"
                                    ? _productoRepositorio.ReservarTabla(item.ProductoId, conexion, transaccion)
                                    : _productoRepositorio.DescontarStock(item.ProductoId, item.Cantidad, item.TipoProducto, conexion, transaccion);

                                if (!reservado)
                                    throw new InvalidOperationException($"'{item.Titulo}' ya no está disponible.");

                                pedido.Items.Add(new PedidoItem
                                {
                                    ProductoId = item.ProductoId,
                                    TituloSnapshot = item.Titulo,
                                    PrecioUnitarioSnapshot = item.Precio,
                                    Cantidad = item.Cantidad
                                });

                                total += item.Precio * item.Cantidad;
                            }

                            pedido.Total = total;
                            pedido.ComisionPlataforma = total * (double)_comision;

                            pedido.Id = _pedidoRepositorio.Insertar(pedido, conexion, transaccion);

                            foreach (CarritoItemDetallado item in itemsShaper)
                                _carritoRepositorio.EliminarItem(clienteId, item.ProductoId, conexion, transaccion);

                            transaccion.Commit();
                        }
                        catch
                        {
                            transaccion.Rollback();
                            throw;
                        }
                    }

                    // Acá "pedido" sigue en scope porque se declaró afuera del using de arriba.
                    string urlPago = await _mercadoPagoServicio.CrearPreferenciaAsync(pedido);
                    _pedidoRepositorio.GuardarPreferenceId(pedido.Id, urlPago);

                    resultado.Add((pedido, urlPago));
                }
            }

            return resultado;
        }

        public async Task ProcesarNotificacionPagoAsync(string mercadoPagoPaymentId)
        {
            // Ver nota debajo del código — falta definir con qué token
            // se consulta el estado del pago.
            throw new NotImplementedException();
        }
    }
}
