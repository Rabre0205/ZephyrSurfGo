using ClassLibrary.Productos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Datos
{
    public interface IProductoRepositorio
    {
        List<Producto> ObtenerTodos();
        List<Producto> ObtenerPorShaper(int shaperId);
        int InsertarLeash(Leash leash);
        int InsertarPad(Pad pad);
        int InsertarQuilla(Quilla quilla);
        int InsertarTabla(Tabla tabla);
        int InsertarTraje(Traje traje);
    }
}
