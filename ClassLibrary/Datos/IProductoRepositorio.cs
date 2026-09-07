using System.Collections.Generic;
using System.Data.SqlClient;
using ClassLibrary.Productos;

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
        bool ReservarTabla(int productoId, SqlConnection conexion, SqlTransaction transaccion);
        bool DescontarStock(int productoId, int cantidad, string tipoProducto, SqlConnection conexion, SqlTransaction transaccion);
    }
}
