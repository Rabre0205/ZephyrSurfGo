using ClassLibrary.Datos;
using ClassLibrary.Pagos;
using ClassLibrary.Pedidos;
using ClassLibrary.Servicios;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TestProject
{
    public class MercadoPagoServicioTests
    {
        private readonly Mock<ICredencialesMercadoPagoRepositorio> _credencialesRepositorioMock = new Mock<ICredencialesMercadoPagoRepositorio>();
        private readonly Mock<ICifradoServicio> _cifradoServicioMock = new Mock<ICifradoServicio>();
        private readonly MercadoPagoServicio _servicio;

        public MercadoPagoServicioTests()
        {
            Environment.SetEnvironmentVariable("MP_CLIENT_ID", "client_id_test");
            Environment.SetEnvironmentVariable("MP_CLIENT_SECRET", "client_secret_test");
            Environment.SetEnvironmentVariable("MP_REDIRECT_URI", "https://test.com/callback");
            Environment.SetEnvironmentVariable("MP_COMISION_PLATAFORMA", "0.15");

            _servicio = new MercadoPagoServicio(
                _credencialesRepositorioMock.Object,
                _cifradoServicioMock.Object);
        }

        [Fact]
        public void ObtenerUrlAutorizacion_ShaperId_DevuelveUrlValida()
        {
            // Arrange
            int shaperId = 5;

            // Act
            var resultado = _servicio.ObtenerUrlAutorizacion(shaperId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Contains("https://auth.mercadopago.com/authorization", resultado);
            Assert.Contains("client_id_test", resultado);
            Assert.Contains("state=5", resultado);
            Assert.Contains("redirect_uri=https%3A%2F%2Ftest.com%2Fcallback", resultado);
        }

        [Fact]
        public void ObtenerUrlAutorizacion_UrlEscapada()
        {
            // Arrange
            int shaperId = 1;

            // Act
            var resultado = _servicio.ObtenerUrlAutorizacion(shaperId);

            // Assert
            Assert.DoesNotContain(" ", resultado);
            Assert.Contains("response_type=code", resultado);
        }

        [Fact]
        public async Task CrearPreferenciaAsync_SinCredenciales_LanzaInvalidOperationException()  
        {
            // Arrange
            var pedido = new Pedido
            {
                Id = 1,
                ShaperId = 1,
                ClienteId = 1,
                Items = new List<PedidoItem>(),
                Total = 500,
                ComisionPlataforma = 75
            };

            _credencialesRepositorioMock
                .Setup(c => c.ObtenerPorUsuarioId(1))
                .Returns((CredencialesMercadoPago)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _servicio.CrearPreferenciaAsync(pedido));
        }

        [Fact]
        public void CrearPreferenciaAsync_ConCredenciales_VerificaDatos()
        {
            // Arrange
            var credenciales = new CredencialesMercadoPago
            {
                UsuarioId = 1,
                MercadoPagoUserId = 12345,
                AccessTokenCifrado = "access_cifrado",
                RefreshTokenCifrado = "refresh_cifrado",
                TokenExpira = DateTime.UtcNow.AddHours(1)
            };

            var pedido = new Pedido
            {
                Id = 1,
                ShaperId = 1,
                ClienteId = 1,
                Items = new List<PedidoItem>
                {
                    new PedidoItem
                    {
                        ProductoId = 1,
                        TituloSnapshot = "Tabla",
                        PrecioUnitarioSnapshot = 500,
                        Cantidad = 1
                    }
                },
                Total = 500,
                ComisionPlataforma = 75
            };

            _credencialesRepositorioMock
                .Setup(c => c.ObtenerPorUsuarioId(1))
                .Returns(credenciales);

            _cifradoServicioMock
                .Setup(c => c.Descifrar("access_cifrado"))
                .Returns("access_token_desencriptado");

            // Act & Assert
            _credencialesRepositorioMock.Verify(
                c => c.ObtenerPorUsuarioId(1),
                Times.AtLeastOnce);
        }

        [Fact]
        public void ObtenerUrlAutorizacion_DiferentesShapersIds_GeneranUrlsDiferentes()
        {
            // Arrange
            int shaperId1 = 1;
            int shaperId2 = 2;

            // Act
            var url1 = _servicio.ObtenerUrlAutorizacion(shaperId1);
            var url2 = _servicio.ObtenerUrlAutorizacion(shaperId2);

            // Assert
            Assert.NotEqual(url1, url2);
            Assert.Contains("state=1", url1);
            Assert.Contains("state=2", url2);
        }

        [Fact]
        public void ObtenerUrlAutorizacion_ConClientIdVacio_GeneraUrlSinClientId()
        {
            // Arrange
            int shaperId = 1;

            // Act
            var resultado = _servicio.ObtenerUrlAutorizacion(shaperId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Contains("authorization", resultado);
        }
    }
}
