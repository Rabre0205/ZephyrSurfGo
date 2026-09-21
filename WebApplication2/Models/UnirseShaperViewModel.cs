using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models;

public class UnirseShaperViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre."), StringLength(100)]
    [Display(Name = "Nombre y apellido")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu correo."), EmailAddress(ErrorMessage = "Ingresá un correo válido."), StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el nombre de tu marca o taller."), StringLength(150)]
    [Display(Name = "Marca o taller")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indicá tu país y ciudad."), StringLength(120)]
    [Display(Name = "País y ciudad")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu número de celular."), Phone(ErrorMessage = "Ingresá un número de celular válido."), StringLength(40)]
    [Display(Name = "Celular o WhatsApp")]
    public string Celular { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el Instagram de tu marca."), StringLength(100)]
    [Display(Name = "Instagram")]
    public string Instagram { get; set; } = string.Empty;

    [Range(0, 80, ErrorMessage = "Ingresá una cantidad de años válida.")]
    [Display(Name = "Años de experiencia")]
    public int AniosExperiencia { get; set; }

    [Display(Name = "Realizo tablas completamente personalizadas")]
    public bool RealizaPersonalizadas { get; set; }

    [Display(Name = "Puedo realizar envíos")]
    public bool RealizaEnvios { get; set; }

    [Required(ErrorMessage = "Contanos brevemente sobre tu trabajo."), StringLength(2000, MinimumLength = 20, ErrorMessage = "La presentación debe tener entre 20 y 2000 caracteres.")]
    [Display(Name = "Sobre tu trabajo")]
    public string Presentacion { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Debés aceptar que te contactemos.")]
    public bool AceptaContacto { get; set; }
}
