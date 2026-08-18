using ClassLibrary.Persona;
using System.Collections.Generic;

namespace WebApplication2.Models
{
    public class ShapersCatalogoViewModel
    {
        public List<ShaperCatalogoItemViewModel> Shapers { get; set; } = new();
    }

    public class ShaperCatalogoItemViewModel
    {
        public Shaper Shaper { get; set; } = null!;
        public int CantidadProductos { get; set; }
    }
}
