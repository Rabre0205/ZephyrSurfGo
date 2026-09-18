using ClassLibrary.Datos;
using ClassLibrary.Enums;
using ClassLibrary.Productos;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TestProject
{
    public class ProductoServicioTests
    {
        private readonly Mock<IProductoRepositorio> _repositorioMock = new Mock<IProductoRepositorio>();
        private readonly Mock<ICloudinaryServicio> _cloudinarioServicioMock = new Mock<ICloudinaryServicio>();
        private readonly ProductoServicio _servicio;

        public ProductoServicioTests()
        {
            _servicio = new ProductoServicio(_repositorioMock.Object, _cloudinarioServicioMock.Object);
        }

        [Fact]
        public void BuscarPorShaper_ShaperId_DevuelveLista()
        {
            // Arrange
            int shaperId = 1;
            var productos = new List<Producto>
            {
                new Producto(1, "Tabla 1", "Subtítulo", 500, "Descripción", "url", shaperId),
                new Producto(2, "Leash", "Leash rápido", 50, "Descripción", "url", shaperId)
            };

            _repositorioMock
                .Setup(r => r.ObtenerPorShaper(shaperId))
                .Returns(productos);

            // Act
            var resultado = _servicio.BuscarPorShaper(shaperId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            _repositorioMock.Verify(r => r.ObtenerPorShaper(shaperId), Times.Once);
        }

        [Fact]
        public void BuscarPorShaper_SinProductos_DevuelveVacio()
        {
            // Arrange
            int shaperId = 99;
            _repositorioMock
                .Setup(r => r.ObtenerPorShaper(shaperId))
                .Returns(new List<Producto>());

            // Act
            var resultado = _servicio.BuscarPorShaper(shaperId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public void ObtenerTablasDelShaper_ConTablas_DevuelveSoloTablas()
        {
            // Arrange
            int shaperId = 1;
            var productos = new List<Producto>
    {
        new Tabla(1, "Tabla Performance", "Subtítulo", 600, "Descripción", "url", shaperId,
            "5'8\"", 20, 28.5, SistemaDeEncaje.FSS2, TipoDeOla.Power, EstiloDeSurf.Agresivo,
            60, 75, Experiencia.Intermedio, "url_atras"),
        new Leash(2, "Leash", "Leash rápido", 50, "Descripción", "url", shaperId, 8),
        new Tabla(3, "Tabla Cruiser", "Subtítulo", 500, "Descripción", "url", shaperId,
            "6'0\"", 22, 32, SistemaDeEncaje.Future, TipoDeOla.Chica, EstiloDeSurf.Recreativo,
            70, 85, Experiencia.Iniciado, "url_atras")
    };

            _repositorioMock
                .Setup(r => r.ObtenerPorShaper(shaperId))
                .Returns(productos);

            // Act
            var resultado = _servicio.ObtenerTablasDelShaper(shaperId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);  // ✅ Verifica exactamente 2
            Assert.All(resultado, tabla => Assert.IsType<Tabla>(tabla));

            // ✅ AGREGADO: Verificar que BuscarPorShaper fue llamado
            _repositorioMock.Verify(r => r.ObtenerPorShaper(shaperId), Times.Once);
        }

        [Fact]
        public void ObtenerTablasDelShaper_SinTablas_DevuelveVacio()
        {
            // Arrange
            int shaperId = 1;
            var productos = new List<Producto>
            {
                new Leash(1, "Leash", "Leash rápido", 50, "Descripción", "url", shaperId, 8),
                new Pad(2, "Pad", "Pad cómodo", 30, "Descripción", "url", shaperId, 100, 50, "EVA")
            };

            _repositorioMock
                .Setup(r => r.ObtenerPorShaper(shaperId))
                .Returns(productos);

            // Act
            var resultado = _servicio.ObtenerTablasDelShaper(shaperId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public void AgregarTabla_SinImagenFrontal_LanzaArgumentException()
        {
            // Arrange
            var imagenMock = new Mock<IFormFile>();
            imagenMock.Setup(f => f.Length).Returns((long)0);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _servicio.AgregarTabla(
                    "Tabla Nueva", "Subtítulo", 500, "Descripción", 1,
                    "5'8\"", 20, 28.5,
                    SistemaDeEncaje.FSS2, TipoDeOla.Power, EstiloDeSurf.Agresivo,
                    60, 75, Experiencia.Intermedio,
                    imagenMock.Object, null));
        }

        [Fact]
        public void AgregarTabla_ConImagenesValidas_LlamaCloudinarioDosVeces()
        {
            // Arrange
            var imagenFrontalMock = new Mock<IFormFile>();
            imagenFrontalMock.Setup(f => f.Length).Returns((long)1000);
            imagenFrontalMock.Setup(f => f.FileName).Returns("imagen.jpg");
            imagenFrontalMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var imagenTraseraMock = new Mock<IFormFile>();
            imagenTraseraMock.Setup(f => f.Length).Returns((long)1000);
            imagenTraseraMock.Setup(f => f.FileName).Returns("imagen_trasera.jpg");
            imagenTraseraMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            _cloudinarioServicioMock
                .Setup(c => c.SubirImagen(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .Returns("https://cloudinary.com/imagen.jpg");

            _repositorioMock
                .Setup(r => r.InsertarTabla(It.IsAny<Tabla>()))
                .Returns(1);

            // Act
            var resultado = _servicio.AgregarTabla(
                "Tabla Nueva", "Subtítulo", 500, "Descripción", 1,
                "5'8\"", 20, 28.5,
                SistemaDeEncaje.FSS2, TipoDeOla.Power, EstiloDeSurf.Agresivo,
                60, 75, Experiencia.Intermedio,
                imagenFrontalMock.Object, imagenTraseraMock.Object);

            // Assert
            Assert.Equal(1, resultado);
            _cloudinarioServicioMock.Verify(
                c => c.SubirImagen(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Exactly(2));
            _repositorioMock.Verify(r => r.InsertarTabla(It.IsAny<Tabla>()), Times.Once);
        }

        [Fact]
        public void AgregarTabla_ConSoloImagenFrontal_LlamaCloudinarioUnaVez()
        {
            // Arrange
            var imagenFrontalMock = new Mock<IFormFile>();
            imagenFrontalMock.Setup(f => f.Length).Returns((long)1000);
            imagenFrontalMock.Setup(f => f.FileName).Returns("imagen.jpg");
            imagenFrontalMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            _cloudinarioServicioMock
                .Setup(c => c.SubirImagen(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .Returns("https://cloudinary.com/imagen.jpg");

            _repositorioMock
                .Setup(r => r.InsertarTabla(It.IsAny<Tabla>()))
                .Returns(1);

            // Act
            var resultado = _servicio.AgregarTabla(
                "Tabla Nueva", "Subtítulo", 500, "Descripción", 1,
                "5'8\"", 20, 28.5,
                SistemaDeEncaje.FSS2, TipoDeOla.Power, EstiloDeSurf.Agresivo,
                60, 75, Experiencia.Intermedio,
                imagenFrontalMock.Object, null);

            // Assert
            Assert.Equal(1, resultado);
            _cloudinarioServicioMock.Verify(
                c => c.SubirImagen(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
