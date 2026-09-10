namespace ClassLibrary.Interacciones;

public class DisenoGuardado
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ShaperId { get; set; }
    public string Nombre { get; set; } = "";
    public string ConfiguracionJson { get; set; } = "{}";
    public DateTime FechaCreacion { get; set; }
}
