using ClassLibrary.Enums;

namespace ClassLibrary.Persona
{
    public class ClienteAdminItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Pais Pais { get; set; }
        public bool Activo { get; set; }
        public int TotalPedidos { get; set; }
        public decimal GastoTotal { get; set; }
    }
}
