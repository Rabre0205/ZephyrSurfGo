using ClassLibrary.Enums;
using System.ComponentModel.DataAnnotations;
using WebApplication2.Models.Configuracion;

namespace Pruebas;

public class ValidacionConfiguracionTests
{
    [Fact]
    public void CuentaValidaAceptaDatosCompletos()
    {
        var modelo = new ConfiguracionCuentaViewModel
        {
            Nombre = "Cliente de prueba",
            Email = "cliente@ejemplo.com",
            Pais = Pais.Uruguay
        };

        Assert.Empty(Validar(modelo));
    }

    [Fact]
    public void CuentaRechazaCorreoInvalidoYNombreVacio()
    {
        var modelo = new ConfiguracionCuentaViewModel
        {
            Nombre = string.Empty,
            Email = "correo-invalido",
            Pais = Pais.Uruguay
        };

        var errores = Validar(modelo);

        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(modelo.Nombre)));
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(modelo.Email)));
    }

    [Fact]
    public void ContraseniaRechazaUnaClaveCorta()
    {
        var modelo = new CambiarContraseniaViewModel
        {
            ContraseniaActual = "Actual123",
            NuevaContrasenia = "Corta1",
            ConfirmarContrasenia = "Corta1"
        };

        var errores = Validar(modelo);

        Assert.Contains(errores, error =>
            error.MemberNames.Contains(nameof(modelo.NuevaContrasenia)));
    }

    [Fact]
    public void ContraseniaRechazaConfirmacionDiferente()
    {
        var modelo = new CambiarContraseniaViewModel
        {
            ContraseniaActual = "Actual123",
            NuevaContrasenia = "Nueva1234",
            ConfirmarContrasenia = "Distinta123"
        };

        var errores = Validar(modelo);

        Assert.Contains(errores, error =>
            error.MemberNames.Contains(nameof(modelo.ConfirmarContrasenia)));
    }

    private static List<ValidationResult> Validar(object modelo)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(
            modelo,
            new ValidationContext(modelo),
            resultados,
            validateAllProperties: true);

        return resultados;
    }
}
