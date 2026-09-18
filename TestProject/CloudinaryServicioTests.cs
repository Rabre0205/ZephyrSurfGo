using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.IO;
using System.Net;
using Xunit;

namespace TestProject
{
    public class CloudinaryServicioTests
    {
        private readonly Mock<Cloudinary> _cloudinaryMock = new Mock<Cloudinary>();
        private readonly CloudinaryServicio _servicio;

        public CloudinaryServicioTests()
        {
            _servicio = new CloudinaryServicio(_cloudinaryMock.Object);
        }

        [Fact]
        public void SubirImagen_ArchivoVacio_LanzaArgumentException()
        {
            // Arrange
            var imagenMock = new Mock<IFormFile>();
            imagenMock.Setup(f => f.Length).Returns((long)0);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _servicio.SubirImagen(imagenMock.Object, "nombre_publico"));
        }

        [Fact]
        public void SubirImagen_ArchivoNull_LanzaArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _servicio.SubirImagen(null, "nombre_publico"));
        }

        [Fact]
        public void SubirImagen_UploadExitoso_DevuelveUrl()
        {
            // Arrange
            var imagenMock = new Mock<IFormFile>();
            imagenMock.Setup(f => f.Length).Returns((long)5000);
            imagenMock.Setup(f => f.FileName).Returns("test.jpg");
            imagenMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[5000]));

            var uploadResult = new ImageUploadResult
            {
                StatusCode = HttpStatusCode.OK,
                SecureUrl = new Uri("https://res.cloudinary.com/test/image/upload/v123/test.jpg")
            };

            _cloudinaryMock
                .Setup(c => c.Upload(It.IsAny<ImageUploadParams>()))
                .Returns(uploadResult);

            // Act
            var resultado = _servicio.SubirImagen(imagenMock.Object, "nombre_publico");

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("https://res.cloudinary.com/test/image/upload/v123/test.jpg", resultado);
        }

        [Fact]
        public void SubirImagen_UploadFalla_LanzaException()
        {
            // Arrange
            var imagenMock = new Mock<IFormFile>();
            imagenMock.Setup(f => f.Length).Returns((long)5000);
            imagenMock.Setup(f => f.FileName).Returns("test.jpg");
            imagenMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[5000]));

            var uploadResult = new ImageUploadResult
            {
                StatusCode = HttpStatusCode.BadRequest,
                Error = new Error { Message = "Archivo inválido" }
            };

            _cloudinaryMock
                .Setup(c => c.Upload(It.IsAny<ImageUploadParams>()))
                .Returns(uploadResult);

            // Act & Assert
            Assert.Throws<Exception>(() =>
                _servicio.SubirImagen(imagenMock.Object, "nombre_publico"));
        }

        [Fact]
        public void SubirImagen_NombrePublicoEnUrl_VerificaNombreEnResultado()
        {
            // Arrange
            var imagenMock = new Mock<IFormFile>();
            imagenMock.Setup(f => f.Length).Returns((long)5000);
            imagenMock.Setup(f => f.FileName).Returns("test.jpg");
            imagenMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[5000]));

            var uploadResult = new ImageUploadResult
            {
                StatusCode = HttpStatusCode.OK,
                SecureUrl = new Uri("https://res.cloudinary.com/test/image/upload/v123/nombre_personalizado")
            };

            _cloudinaryMock
                .Setup(c => c.Upload(It.IsAny<ImageUploadParams>()))
                .Returns(uploadResult);

            // Act
            var resultado = _servicio.SubirImagen(imagenMock.Object, "nombre_personalizado");

            // Assert
            Assert.NotNull(resultado);
            Assert.Contains("nombre_personalizado", resultado);  // ✅ Verifica que está en la URL
            _cloudinaryMock.Verify(
                c => c.Upload(It.Is<ImageUploadParams>(p =>
                    p.PublicId == "nombre_personalizado" && p.Overwrite == true)),
                Times.Once);
        }
    }
}
