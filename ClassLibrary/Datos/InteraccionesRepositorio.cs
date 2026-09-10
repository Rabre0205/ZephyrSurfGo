using ClassLibrary.Interacciones;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface IInteraccionesRepositorio
{
    bool PuedeResenar(int clienteId, int pedidoId, int productoId);
    bool GuardarResena(int clienteId, int pedidoId, int productoId, byte estrellas, string comentario);
    List<ResenaProducto> ObtenerResenas(int productoId);
    Dictionary<int, ResumenResenas> ObtenerResumenes(IEnumerable<int> productoIds);
    bool AlternarFavorito(int clienteId, int productoId);
    bool EsFavorito(int clienteId, int productoId);
    List<FavoritoProducto> ObtenerFavoritos(int clienteId);
    int GuardarDiseno(DisenoGuardado diseno);
    List<DisenoGuardado> ObtenerDisenos(int clienteId);
    DisenoGuardado? ObtenerDiseno(int id, int clienteId);
    bool EliminarDiseno(int id, int clienteId);
}

public class InteraccionesRepositorio : IInteraccionesRepositorio
{
    public bool PuedeResenar(int clienteId, int pedidoId, int productoId)
    {
        const string sql = @"SELECT COUNT(*) FROM Pedidos p INNER JOIN PedidoItems i ON i.PedidoId=p.Id
            WHERE p.Id=@PedidoId AND p.ClienteId=@ClienteId AND p.EstadoPedidoId=4 AND i.ProductoId=@ProductoId;";
        using var cn = Conexion.ObtenerConexion(); using var cmd = new SqlCommand(sql, cn);
        ParametrosCompra(cmd, clienteId, pedidoId, productoId); cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public bool GuardarResena(int clienteId, int pedidoId, int productoId, byte estrellas, string comentario)
    {
        if (!PuedeResenar(clienteId, pedidoId, productoId)) return false;
        const string sql = @"IF NOT EXISTS(SELECT 1 FROM ResenasProductos WHERE ClienteId=@ClienteId AND ProductoId=@ProductoId)
            BEGIN INSERT INTO ResenasProductos(ProductoId,ClienteId,PedidoId,Estrellas,Comentario)
            VALUES(@ProductoId,@ClienteId,@PedidoId,@Estrellas,@Comentario); SELECT 1; END ELSE SELECT 0;";
        using var cn = Conexion.ObtenerConexion(); using var cmd = new SqlCommand(sql, cn);
        ParametrosCompra(cmd, clienteId, pedidoId, productoId);
        cmd.Parameters.Add("@Estrellas", SqlDbType.TinyInt).Value = estrellas;
        cmd.Parameters.Add("@Comentario", SqlDbType.NVarChar, 1000).Value = comentario;
        cn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public List<ResenaProducto> ObtenerResenas(int productoId)
    {
        const string sql = @"SELECT r.Id,r.ProductoId,r.ClienteId,u.Nombre ClienteNombre,r.Estrellas,r.Comentario,r.FechaCreacion
            FROM ResenasProductos r INNER JOIN Usuarios u ON u.Id=r.ClienteId WHERE r.ProductoId=@ProductoId
            ORDER BY r.FechaCreacion DESC;";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn);
        cmd.Parameters.Add("@ProductoId",SqlDbType.Int).Value=productoId; cn.Open(); using var rd=cmd.ExecuteReader();
        var lista=new List<ResenaProducto>(); while(rd.Read()) lista.Add(MapearResena(rd)); return lista;
    }

    public Dictionary<int, ResumenResenas> ObtenerResumenes(IEnumerable<int> productoIds)
    {
        var ids=productoIds.Distinct().ToArray(); var resultado=new Dictionary<int,ResumenResenas>(); if(ids.Length==0)return resultado;
        var nombres=ids.Select((_,i)=>"@Id"+i).ToArray();
        string sql=$"SELECT ProductoId,AVG(CAST(Estrellas AS FLOAT)) Promedio,COUNT(*) Cantidad FROM ResenasProductos WHERE ProductoId IN ({string.Join(",",nombres)}) GROUP BY ProductoId;";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn);
        for(int i=0;i<ids.Length;i++)cmd.Parameters.Add(nombres[i],SqlDbType.Int).Value=ids[i]; cn.Open(); using var rd=cmd.ExecuteReader();
        while(rd.Read())resultado[Convert.ToInt32(rd["ProductoId"])]=new(Convert.ToDouble(rd["Promedio"]),Convert.ToInt32(rd["Cantidad"])); return resultado;
    }

    public bool AlternarFavorito(int clienteId,int productoId)
    {
        const string sql=@"IF EXISTS(SELECT 1 FROM FavoritosProductos WHERE ClienteId=@ClienteId AND ProductoId=@ProductoId)
            BEGIN DELETE FROM FavoritosProductos WHERE ClienteId=@ClienteId AND ProductoId=@ProductoId; SELECT 0; END
            ELSE IF EXISTS(SELECT 1 FROM Productos WHERE Id=@ProductoId AND DELETED=0)
            BEGIN INSERT INTO FavoritosProductos(ClienteId,ProductoId) VALUES(@ClienteId,@ProductoId); SELECT 1; END
            ELSE SELECT 0;";
        using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);AgregarClienteProducto(cmd,clienteId,productoId);cn.Open();return Convert.ToInt32(cmd.ExecuteScalar())==1;
    }
    public bool EsFavorito(int clienteId,int productoId){const string sql="SELECT COUNT(*) FROM FavoritosProductos WHERE ClienteId=@ClienteId AND ProductoId=@ProductoId";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);AgregarClienteProducto(cmd,clienteId,productoId);cn.Open();return Convert.ToInt32(cmd.ExecuteScalar())>0;}
    public List<FavoritoProducto> ObtenerFavoritos(int clienteId){const string sql=@"SELECT p.Id ProductoId,p.ShaperId,p.Titulo,p.Subtitulo,p.ImagenUrl,p.Precio,p.TipoProducto,f.FechaCreacion FROM FavoritosProductos f INNER JOIN Productos p ON p.Id=f.ProductoId WHERE f.ClienteId=@ClienteId AND p.DELETED=0 ORDER BY f.FechaCreacion DESC";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cn.Open();using var rd=cmd.ExecuteReader();var l=new List<FavoritoProducto>();while(rd.Read())l.Add(new(){ProductoId=Convert.ToInt32(rd["ProductoId"]),ShaperId=Convert.ToInt32(rd["ShaperId"]),Titulo=Convert.ToString(rd["Titulo"])??"",Subtitulo=Convert.ToString(rd["Subtitulo"])??"",ImagenUrl=Convert.ToString(rd["ImagenUrl"])??"",Precio=Convert.ToDecimal(rd["Precio"]),TipoProducto=Convert.ToString(rd["TipoProducto"])??"",FechaCreacion=Convert.ToDateTime(rd["FechaCreacion"])});return l;}

    public int GuardarDiseno(DisenoGuardado d){const string sql=@"INSERT INTO DisenosGuardados(ClienteId,ShaperId,Nombre,ConfiguracionJson) OUTPUT INSERTED.Id VALUES(@ClienteId,@ShaperId,@Nombre,@Json)";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=d.ClienteId;cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=d.ShaperId;cmd.Parameters.Add("@Nombre",SqlDbType.NVarChar,100).Value=d.Nombre;cmd.Parameters.Add("@Json",SqlDbType.NVarChar,-1).Value=d.ConfiguracionJson;cn.Open();return Convert.ToInt32(cmd.ExecuteScalar());}
    public List<DisenoGuardado> ObtenerDisenos(int clienteId)=>ObtenerDisenosInterno("ClienteId=@ClienteId",clienteId);
    public DisenoGuardado? ObtenerDiseno(int id,int clienteId)=>ObtenerDisenosInterno("Id=@Id AND ClienteId=@ClienteId",clienteId,id).SingleOrDefault();
    public bool EliminarDiseno(int id,int clienteId){const string sql="DELETE FROM DisenosGuardados WHERE Id=@Id AND ClienteId=@ClienteId";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cn.Open();return cmd.ExecuteNonQuery()==1;}
    private static List<DisenoGuardado> ObtenerDisenosInterno(string filtro,int clienteId,int? id=null){string sql=$"SELECT Id,ClienteId,ShaperId,Nombre,ConfiguracionJson,FechaCreacion FROM DisenosGuardados WHERE {filtro} ORDER BY FechaCreacion DESC";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;if(id.HasValue)cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id.Value;cn.Open();using var rd=cmd.ExecuteReader();var l=new List<DisenoGuardado>();while(rd.Read())l.Add(new(){Id=Convert.ToInt32(rd["Id"]),ClienteId=Convert.ToInt32(rd["ClienteId"]),ShaperId=Convert.ToInt32(rd["ShaperId"]),Nombre=Convert.ToString(rd["Nombre"])??"",ConfiguracionJson=Convert.ToString(rd["ConfiguracionJson"])??"{}",FechaCreacion=Convert.ToDateTime(rd["FechaCreacion"])});return l;}
    private static void AgregarClienteProducto(SqlCommand c,int clienteId,int productoId){c.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;c.Parameters.Add("@ProductoId",SqlDbType.Int).Value=productoId;}
    private static void ParametrosCompra(SqlCommand c,int clienteId,int pedidoId,int productoId){AgregarClienteProducto(c,clienteId,productoId);c.Parameters.Add("@PedidoId",SqlDbType.Int).Value=pedidoId;}
    private static ResenaProducto MapearResena(SqlDataReader r)=>new(){Id=Convert.ToInt32(r["Id"]),ProductoId=Convert.ToInt32(r["ProductoId"]),ClienteId=Convert.ToInt32(r["ClienteId"]),ClienteNombre=Convert.ToString(r["ClienteNombre"])??"",Estrellas=Convert.ToByte(r["Estrellas"]),Comentario=Convert.ToString(r["Comentario"])??"",FechaCreacion=Convert.ToDateTime(r["FechaCreacion"])};
}
