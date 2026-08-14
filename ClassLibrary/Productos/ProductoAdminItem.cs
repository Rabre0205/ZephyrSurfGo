namespace ClassLibrary.Productos
{
    public class ProductoAdminItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string TipoProducto { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int ShaperId { get; set; }
        public string ShaperNombre { get; set; } = string.Empty;
        public string NegocioShaper { get; set; } = string.Empty;
        public int? Stock { get; set; }
        public bool Disponible { get; set; }
        public bool Oculto { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
    }
}
