using ClassLibrary.Carrito;
using ClassLibrary.Datos;
using ClassLibrary.Pedidos;
using ClassLibrary.Servicios;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace TestProject
{
    public class PedidoServicioTests
    {
        private readonly Mock<ICarritoRepositorio> _carritoRepositorioMock = new Mock<ICarritoRepositorio>();
        private readonly Mock<IProductoRepositorio> _productoRepositorioMock = new Mock<IProductoRepositorio>();
        private readonly Mock<IPedidoRepositorio> _pedidoRepositorioMock = new Mock<IPedidoRepositorio>();
        private readonly Mock<IMercadoPagoServicio> _mercadoPagoServicioMock = new Mock<IMercadoPagoServicio>();
        private readonly PedidoServicio _servicio;

        public PedidoServicioTests()
        {
            Environment.SetEnvironmentVariable("MP_COMISION_PLATAFORMA", "0.15");

            _servicio = new PedidoServicio(
                _carritoRepositorioMock.Object,
                _productoRepositorioMock.Object,
                _pedidoRepositorioMock.Object,
                _mercadoPagoServicioMock.Object);
        }

        [Fact]
        public async Task CrearPedidosDesdeCarritoAsync_CarritoVacio_LanzaInvalidOperationException()
        {
            // Arrange
            int clienteId = 1;
            _carritoRepositorioMock
                .Setup(c => c.ObtenerPorUsuario(clienteId))
                .Returns(new List<CarritoItemDetallado>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicio.CrearPedidosDesdeCarritoAsync(clienteId));
        }

        [Fact]
        public async Task CrearPedidosDesdeCarritoAsync_ConItems_CreaUnPedidoPorShaper()
        {
            // Arrange
            int clienteId = 1;
            var items = new List<CarritoItemDetallado>
            {
                new CarritoItemDetallado
                {
                    ProductoId = 1,
                    ShaperId = 1,
                    Titulo = "Tabla",
                    Precio = 500,
                    Cantidad = 1,
                    TipoProducto = "Tabla"
                },
                new CarritoItemDetallado
                {
                    ProductoId = 2,
                    ShaperId = 1,
                    Titulo = "Leash",
                    Precio = 50,
                    Cantidad = 2,
                    TipoProducto = "Leash"
                }
            };

            _carritoRepositorioMock
                .Setup(c => c.ObtenerPorUsuario(clienteId))
                .Returns(items);

            _productoRepositorioMock
                .Setup(p => p.ReservarTabla(It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(true);

            _productoRepositorioMock
                .Setup(p => p.DescontarStock(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(true);

            _pedidoRepositorioMock
                .Setup(p => p.Insertar(It.IsAny<Pedido>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(1);

            // Act
            var resultado = await _servicio.CrearPedidosDesdeCarritoAsync(clienteId);

            // Assert
            Assert.NotNull(resultado);
            Assert.NotEmpty(resultado);
            _pedidoRepositorioMock.Verify(
                p => p.Insertar(It.IsAny<Pedido>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()),
                Times.Once);
            _carritoRepositorioMock.Verify(
                c => c.EliminarItem(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task CrearPedidosDesdeCarritoAsync_ItemsNoDisponibles_LanzaInvalidOperationException()
        {
            // Arrange
            int clienteId = 1;
            var items = new List<CarritoItemDetallado>
            {
                new CarritoItemDetallado
                {
                    ProductoId = 1,
                    ShaperId = 1,
                    Titulo = "Tabla",
                    Precio = 500,
                    Cantidad = 1,
                    TipoProducto = "Tabla"
                }
            };

            _carritoRepositorioMock
                .Setup(c => c.ObtenerPorUsuario(clienteId))
                .Returns(items);

            _productoRepositorioMock
                .Setup(p => p.ReservarTabla(It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _servicio.CrearPedidosDesdeCarritoAsync(clienteId));
        }

        [Fact]
        public async Task CrearPedidosDesdeCarritoAsync_CalculaComisionConMultiplesCantidades()
        {
            // Arrange
            int clienteId = 1;
            var items = new List<CarritoItemDetallado>
    {
        new CarritoItemDetallado
        {
            ProductoId = 1,
            ShaperId = 1,
            Titulo = "Tabla",
            Precio = 500,
            Cantidad = 2,  // ✅ 2 unidades
            TipoProducto = "Tabla"
        },
        new CarritoItemDetallado
        {
            ProductoId = 2,
            ShaperId = 1,
            Titulo = "Leash",
            Precio = 100,
            Cantidad = 3,  // ✅ 3 unidades
            TipoProducto = "Leash"
        }
    };
            // Total esperado: (500*2) + (100*3) = 1000 + 300 = 1300
            // Comisión esperada: 1300 * 0.15 = 195

            _carritoRepositorioMock.Setup(c => c.ObtenerPorUsuario(clienteId)).Returns(items);
            _productoRepositorioMock
                .Setup(p => p.ReservarTabla(It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(true);
            _productoRepositorioMock
                .Setup(p => p.DescontarStock(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(true);
            _pedidoRepositorioMock
                .Setup(p => p.Insertar(It.IsAny<Pedido>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(1);

            // Act
            await _servicio.CrearPedidosDesdeCarritoAsync(clienteId);

            // Assert
            _pedidoRepositorioMock.Verify(
                p => p.Insertar(
                    It.Is<Pedido>(ped =>
                        ped.Total == 1300 &&  // ✅ Total correcto
                        ped.ComisionPlataforma == 195 &&  // ✅ Comisión correcta
                        ped.Items.Count == 2),  // ✅ 2 items en el pedido
                    It.IsAny<SqlConnection>(),
                    It.IsAny<SqlTransaction>()),
                Times.Once);
        }

        [Fact]
        public async Task CrearPedidosDesdeCarritoAsync_GrupaItemsPorShaper()
        {
            // Arrange
            int clienteId = 1;
            var items = new List<CarritoItemDetallado>
            {
                new CarritoItemDetallado
                {
                    ProductoId = 1,
                    ShaperId = 1,
                    Titulo = "Tabla Shaper 1",
                    Precio = 500,
                    Cantidad = 1,
                    TipoProducto = "Tabla"
                },
                new CarritoItemDetallado
                {
                    ProductoId = 2,
                    ShaperId = 2,
                    Titulo = "Tabla Shaper 2",
                    Precio = 600,
                    Cantidad = 1,
                    TipoProducto = "Tabla"
                }
            };

            _carritoRepositorioMock
                .Setup(c => c.ObtenerPorUsuario(clienteId))
                .Returns(items);

            _productoRepositorioMock
                .Setup(p => p.ReservarTabla(It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(true);

            _pedidoRepositorioMock
                .Setup(p => p.Insertar(It.IsAny<Pedido>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(1);

            // Act
            var resultado = await _servicio.CrearPedidosDesdeCarritoAsync(clienteId);

            // Assert
            Assert.Equal(2, resultado.Count);
            _pedidoRepositorioMock.Verify(
                p => p.Insertar(It.IsAny<Pedido>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task CrearPedidosDesdeCarritoAsync_ConItems_PedidoConteieneTodosLosItems()
        {
            // Arrange
            int clienteId = 1;
            var items = new List<CarritoItemDetallado>
            {
                new CarritoItemDetallado
                {
                    ProductoId = 1,
                    ShaperId = 1,
                    Titulo = "Tabla Shaper 1",
                    Precio = 500,
                    Cantidad = 1,
                    TipoProducto = "Tabla"
                },
                new CarritoItemDetallado
                {
                    ProductoId = 2,
                    ShaperId = 2,
                    Titulo = "Tabla Shaper 2",
                    Precio = 600,
                    Cantidad = 1,
                    TipoProducto = "Tabla"
                }
            };

            _carritoRepositorioMock
                .Setup(c => c.ObtenerPorUsuario(clienteId))
                .Returns(items);

            _productoRepositorioMock
                .Setup(p => p.ReservarTabla(It.IsAny<int>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(true);

            _pedidoRepositorioMock
                .Setup(p => p.Insertar(It.IsAny<Pedido>(), It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(1);

            // Act
            var resultado = await _servicio.CrearPedidosDesdeCarritoAsync(clienteId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Single(resultado);  // ✅ Un solo pedido para Shaper 1

            var pedido = resultado[0].Pedido;
            Assert.Equal(2, pedido.Items.Count);  // ✅ Verifica que tiene 2 items
            Assert.Equal(600, pedido.Total);  // ✅ Verifica total correcto (500 + 50*2)
            Assert.Equal(90, pedido.ComisionPlataforma);  // ✅ Verifica comisión (600 * 0.15)

            _pedidoRepositorioMock.Verify(...);//... resto igual ...
        }
    }
}
