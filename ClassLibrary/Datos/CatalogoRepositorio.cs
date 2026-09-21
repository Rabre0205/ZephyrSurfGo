using ClassLibrary.Catalogo;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface ICatalogoRepositorio
{
    int Contar(string busqueda, string tipo, decimal? precioMin, decimal? precioMax, bool soloDisponibles);
    List<CatalogoProducto> Buscar(string busqueda, string tipo, decimal? precioMin, decimal? precioMax,
        bool soloDisponibles, string ordenar, int pagina, int cantidad);
}

public class CatalogoRepositorio : ICatalogoRepositorio
{
    private const string Desde = @"
        FROM Productos p
        INNER JOIN Usuarios u ON u.Id=p.ShaperId
        INNER JOIN Paises pa ON pa.Id=u.PaisId
        LEFT JOIN Leashes l ON l.ProductoId=p.Id
        LEFT JOIN Pads pad ON pad.ProductoId=p.Id
        LEFT JOIN Quillas q ON q.ProductoId=p.Id
        LEFT JOIN Tablas t ON t.ProductoId=p.Id
        LEFT JOIN Trajes tr ON tr.ProductoId=p.Id
        OUTER APPLY (SELECT AVG(CAST(r.Estrellas AS FLOAT)) Promedio, COUNT(*) Cantidad
                     FROM ResenasProductos r WHERE r.ProductoId=p.Id) rv
        WHERE p.DELETED=0 AND u.Activo=1
          AND (@Busqueda='' OR p.Titulo LIKE '%'+@Busqueda+'%' OR p.Subtitulo LIKE '%'+@Busqueda+'%'
               OR u.Nombre LIKE '%'+@Busqueda+'%' OR u.NombreDeNegosio LIKE '%'+@Busqueda+'%')
          AND (@Tipo='' OR p.TipoProducto=@Tipo)
          AND (@PrecioMin IS NULL OR p.Precio>=@PrecioMin)
          AND (@PrecioMax IS NULL OR p.Precio<=@PrecioMax)
          AND (@Disponibles=0 OR CASE WHEN p.TipoProducto='Tabla' THEN ISNULL(t.Disponible,0)
              WHEN p.TipoProducto='Leash' AND ISNULL(l.Stock,0)>0 THEN 1
              WHEN p.TipoProducto='Pad' AND ISNULL(pad.Stock,0)>0 THEN 1
              WHEN p.TipoProducto='Quilla' AND ISNULL(q.Stock,0)>0 THEN 1
              WHEN p.TipoProducto='Traje' AND ISNULL(tr.Stock,0)>0 THEN 1 ELSE 0 END=1)";

    public int Contar(string busqueda,string tipo,decimal? precioMin,decimal? precioMax,bool soloDisponibles)
    {
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand("SELECT COUNT(*) "+Desde,cn);
        Agregar(cmd,busqueda,tipo,precioMin,precioMax,soloDisponibles); cn.Open(); return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<CatalogoProducto> Buscar(string busqueda,string tipo,decimal? precioMin,decimal? precioMax,bool soloDisponibles,string ordenar,int pagina,int cantidad)
    {
        string orden=ordenar switch { "precio_asc"=>"p.Precio ASC", "precio_desc"=>"p.Precio DESC", "valorados"=>"ISNULL(rv.Promedio,0) DESC, ISNULL(rv.Cantidad,0) DESC", _=>"p.Id DESC" };
        string sql=@"SELECT p.Id,p.Titulo,p.Subtitulo,p.TipoProducto,p.Precio,p.ImagenUrl,p.ShaperId,
            COALESCE(NULLIF(u.NombreDeNegosio,''),u.Nombre) Shaper,pa.Nombre Pais,
            CASE WHEN p.TipoProducto='Tabla' THEN ISNULL(t.Disponible,0)
              WHEN p.TipoProducto='Leash' AND ISNULL(l.Stock,0)>0 THEN 1 WHEN p.TipoProducto='Pad' AND ISNULL(pad.Stock,0)>0 THEN 1
              WHEN p.TipoProducto='Quilla' AND ISNULL(q.Stock,0)>0 THEN 1 WHEN p.TipoProducto='Traje' AND ISNULL(tr.Stock,0)>0 THEN 1 ELSE 0 END Disponible,
            ISNULL(rv.Promedio,0) Promedio,ISNULL(rv.Cantidad,0) Cantidad "+Desde+" ORDER BY "+orden+" OFFSET @Offset ROWS FETCH NEXT @Cantidad ROWS ONLY";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn); Agregar(cmd,busqueda,tipo,precioMin,precioMax,soloDisponibles);
        cmd.Parameters.Add("@Offset",SqlDbType.Int).Value=(Math.Max(1,pagina)-1)*cantidad; cmd.Parameters.Add("@Cantidad",SqlDbType.Int).Value=cantidad;
        cn.Open(); using var rd=cmd.ExecuteReader(); var items=new List<CatalogoProducto>();
        while(rd.Read())items.Add(new(){Id=Convert.ToInt32(rd["Id"]),Titulo=Convert.ToString(rd["Titulo"])??"",Subtitulo=Convert.ToString(rd["Subtitulo"])??"",Tipo=Convert.ToString(rd["TipoProducto"])??"",Precio=Convert.ToDecimal(rd["Precio"]),ImagenUrl=Convert.ToString(rd["ImagenUrl"])??"",ShaperId=Convert.ToInt32(rd["ShaperId"]),Shaper=Convert.ToString(rd["Shaper"])??"",Pais=Convert.ToString(rd["Pais"])??"",Disponible=Convert.ToBoolean(rd["Disponible"]),PromedioResenas=Convert.ToDouble(rd["Promedio"]),CantidadResenas=Convert.ToInt32(rd["Cantidad"])});
        return items;
    }
    private static void Agregar(SqlCommand c,string b,string t,decimal? min,decimal? max,bool d){c.Parameters.Add("@Busqueda",SqlDbType.NVarChar,150).Value=b??"";c.Parameters.Add("@Tipo",SqlDbType.NVarChar,20).Value=t??"";c.Parameters.Add("@PrecioMin",SqlDbType.Decimal).Value=(object?)min??DBNull.Value;c.Parameters.Add("@PrecioMax",SqlDbType.Decimal).Value=(object?)max??DBNull.Value;c.Parameters.Add("@Disponibles",SqlDbType.Bit).Value=d;}
}
