namespace WebApplication2.Models.Productos
{
    public class RegistrarClienteFormModel
    {
        public string Email { get; set; } = string.Empty;
        public string Contrasenia { get; set; } = string.Empty;
        public string ConfirmarContrasenia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public byte Pais { get; set; }
    }
}
