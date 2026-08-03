using ClassLibrary.Enums;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.PanelAdmin
{
    public class EditarShaperViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná un país.")]
        [Display(Name = "País")]
        public Pais Pais { get; set; }

        [Required(ErrorMessage = "El nombre del negocio es obligatorio.")]
        [Display(Name = "Nombre del negocio")]
        public string NombreDeNegosio { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contacto es obligatorio.")]
        [Display(Name = "Contacto")]
        public string Contacto { get; set; } = string.Empty;
    }
}