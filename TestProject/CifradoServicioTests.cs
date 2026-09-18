using ClassLibrary.Servicios;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using System;
using System.Security.Cryptography;
using Xunit;

namespace TestProject
{
    public class CifradoServicioTests
    {
        private readonly Mock<IDataProtectionProvider> _providerMock = new Mock<IDataProtectionProvider>();
        private readonly Mock<IDataProtector> _protectorMock = new Mock<IDataProtector>();
        private readonly CifradoServicio _servicio;

        public CifradoServicioTests()
        {
            _providerMock
                .Setup(p => p.CreateProtector("MercadoPago.Credenciales"))
                .Returns(_protectorMock.Object);

            _servicio = new CifradoServicio(_providerMock.Object);
        }

        [Fact]
        public void Cifrar_TextoPlano_LlamaAlProtector()
        {
            // Arrange
            string textoPlano = "123456789";
            string textoCifrado = "cifrado_encriptado";

            _protectorMock
                .Setup(p => p.Protect(textoPlano))
                .Returns(textoCifrado);

            // Act
            var resultado = _servicio.Cifrar(textoPlano);

            // Assert
            Assert.Equal(textoCifrado, resultado);
            _protectorMock.Verify(p => p.Protect(textoPlano), Times.Once);
        }

        [Fact]
        public void Cifrar_TextoVacio_ReturnsEncrypted()
        {
            // Arrange
            string textoPlano = "";
            string textoCifrado = "cifrado_vacio";

            _protectorMock
                .Setup(p => p.Protect(textoPlano))
                .Returns(textoCifrado);

            // Act
            var resultado = _servicio.Cifrar(textoPlano);

            // Assert
            Assert.NotNull(resultado);
            _protectorMock.Verify(p => p.Protect(textoPlano), Times.Once);
        }

        [Fact]
        public void Descifrar_TextoCifrado_LlamaAlProtector()
        {
            // Arrange
            string textoCifrado = "cifrado_encriptado";
            string textoDescifrado = "123456789";

            _protectorMock
                .Setup(p => p.Unprotect(textoCifrado))
                .Returns(textoDescifrado);

            // Act
            var resultado = _servicio.Descifrar(textoCifrado);

            // Assert
            Assert.Equal(textoDescifrado, resultado);
            _protectorMock.Verify(p => p.Unprotect(textoCifrado), Times.Once);
        }

        [Fact]
        public void Descifrar_TextoInvalido_LanzaException()
        {
            // Arrange
            string textoCifrado = "cifrado_invalido";

            _protectorMock
                .Setup(p => p.Unprotect(textoCifrado))
                .Throws(new CryptographicException("Falló la desencriptación"));

            // Act & Assert
            Assert.Throws<CryptographicException>(() =>
                _servicio.Descifrar(textoCifrado));
        }

        [Fact]
        public void CifrarYDescifrar_CicloCompleto_RecuperaTextoOriginal()
        {
            // Arrange
            string textoOriginal = "AccessToken123";
            string textoCifrado = "cifrado_encriptado";

            _protectorMock
                .Setup(p => p.Protect(textoOriginal))
                .Returns(textoCifrado);

            _protectorMock
                .Setup(p => p.Unprotect(textoCifrado))
                .Returns(textoOriginal);

            // Act
            var cifrado = _servicio.Cifrar(textoOriginal);
            var descifrado = _servicio.Descifrar(cifrado);

            // Assert
            Assert.Equal(textoOriginal, descifrado);
        }

        [Fact]
        public void Cifrar_CreadorProtectorConKeyCorrecta()
        {
            // Act
            var resultado = _servicio.Cifrar("test");

            // Assert
            _providerMock.Verify(
                p => p.CreateProtector("MercadoPago.Credenciales"),
                Times.AtLeastOnce);
        }
    }
}
