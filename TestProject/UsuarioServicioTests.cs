using ClassLibrary.Datos;
using ClassLibrary.Enums;
using ClassLibrary.Persona;
using ClassLibrary.Servicios;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject
{
    public class UsuarioServicioTests
    {
        private readonly Mock<IUsuarioRepositorio> _repositorioMock = new Mock<IUsuarioRepositorio>();
        private readonly Mock<ICloudinaryServicio> _cloudinarioServicioMock = new Mock<ICloudinaryServicio>();
        private readonly UsuarioServicio _servicio;
        public UsuarioServicioTests()
        {
            _servicio = new UsuarioServicio(_repositorioMock.Object, _cloudinarioServicioMock.Object);
        }

        [Fact]
        public void RegistrarCliente_EmailDuplicado_DevuelveError()
        {
            _repositorioMock
                .Setup(r => r.ObtenerPorEmail("existente@test.com"))
                .Returns(new Usuario(1, "existente@test.com", "Juan", Pais.Uruguay, "hash"));

            var resultado = _servicio.RegistrarCliente(
                "existente@test.com", "Juan", Pais.Uruguay, "123456", "123456");

            Assert.False(resultado.Exito);
            Assert.Equal("Ya existe un usuario registrado con ese email.", resultado.Error);
        }

        [Fact]
        public void RegistrarCliente_ContraseniasNoCoinciden_NoLlamaAlRepositorio()
        {
            var resultado = _servicio.RegistrarCliente(
                "nuevo@test.com", "Juan", Pais.Uruguay, "123456", "654321");

            Assert.False(resultado.Exito);
            _repositorioMock.Verify(r => r.InsertarUsuario(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void RegistrarCliente_DatosValidos_GuardaHashNoTextoPlano()
        {
            _repositorioMock.Setup(r => r.ObtenerPorEmail(It.IsAny<string>())).Returns((Usuario)null);
            _repositorioMock.Setup(r => r.InsertarUsuario(It.IsAny<Usuario>())).Returns(1);

            _servicio.RegistrarCliente("nuevo@test.com", "Juan", Pais.Uruguay, "123456", "123456");

            _repositorioMock.Verify(r => r.InsertarUsuario(
                It.Is<Usuario>(u => u.Contrasenia != "123456")), Times.Once);
        }
    }
}
