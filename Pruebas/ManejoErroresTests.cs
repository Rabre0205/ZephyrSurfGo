using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Controllers;
using WebApplication2.Models;

namespace Pruebas;

public class ManejoErroresTests
{
    [Theory]
    [InlineData(403, "Acceso denegado")]
    [InlineData(404, "Página no encontrada")]
    [InlineData(500, "Ocurrió un error")]
    public void ModeloPresentaMensajesAmigables(int codigo, string titulo)
    {
        var modelo = new ErrorViewModel { StatusCode = codigo };

        Assert.Equal(titulo, modelo.Titulo);
        Assert.False(string.IsNullOrWhiteSpace(modelo.Mensaje));
    }

    [Fact]
    public void ControladorConservaElCodigoHttpSolicitado()
    {
        var contexto = new DefaultHttpContext();
        var controlador = new HomeController
        {
            ControllerContext = new ControllerContext { HttpContext = contexto }
        };

        var resultado = Assert.IsType<ViewResult>(controlador.Error(404));
        var modelo = Assert.IsType<ErrorViewModel>(resultado.Model);

        Assert.Equal(404, contexto.Response.StatusCode);
        Assert.Equal(404, modelo.StatusCode);
    }
}
