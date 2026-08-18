using ClassLibrary.Enums;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.Configuracion
{
    public class ConfiguracionViewModel
    {
        public ConfiguracionCuentaViewModel Cuenta { get; set; } = new();
        public CambiarContraseniaViewModel Seguridad { get; set; } = new();
    }

    public class ConfiguracionCuentaViewModel
    {
        [Required(ErrorMessage = "Ingresá tu nombre.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá tu correo.")]
        [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná tu país.")]
        public Pais Pais { get; set; }
    }

    public class CambiarContraseniaViewModel
    {
        [Required(ErrorMessage = "Ingresá tu contraseña actual.")]
        [DataType(DataType.Password)]
        public string ContraseniaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá una contraseña nueva.")]
        [MinLength(8, ErrorMessage = "Debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string NuevaContrasenia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmá la contraseña nueva.")]
        [Compare(nameof(NuevaContrasenia), ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasenia { get; set; } = string.Empty;
    }
}
