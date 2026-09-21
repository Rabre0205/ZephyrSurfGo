using ClassLibrary.Interacciones;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface IInteraccionesRepositorio
{
    bool PuedeResenar(int clienteId, int pedidoId, int productoId);
    bool GuardarResena(int clienteId, int pedidoId, int productoId, byte estrellas, string comentario);
    bool EditarResena(int resenaId, int clienteId, byte estrellas, string comentario);
    bool ResponderResena(int resenaId, int shaperId, string respuesta);
    bool ModerarResena(int resenaId, bool moderada, string motivo);
    ResenaProducto? ObtenerResena(int resenaId);
    List<ResenaProducto> ObtenerResenasAdministracion(string busqueda, string estado);
    List<ResenaProducto> ObtenerResenas(int productoId);
    Dictionary<int, ResumenResenas> ObtenerResumenes(IEnumerable<int> productoIds);
    bool AlternarFavorito(int clienteId, int productoId);
    bool EsFavorito(int clienteId, int productoId);
    List<FavoritoProducto> ObtenerFavoritos(int clienteId);
    int EliminarFavoritos(int clienteId, IEnumerable<int> productoIds);
    int GuardarDiseno(DisenoGuardado diseno);
    List<DisenoGuardado> ObtenerDisenos(int clienteId);
    DisenoGuardado? ObtenerDiseno(int id, int clienteId);
    bool EliminarDiseno(int id, int clienteId);
    int EliminarDisenos(int clienteId, IEnumerable<int> ids);
    bool RenombrarDiseno(int id, int clienteId, string nombre);
    int? DuplicarDiseno(int id, int clienteId, string nombre);
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
        const string sql = @"SELECT r.Id,r.ProductoId,r.ClienteId,u.Nombre ClienteNombre,r.Estrellas,r.Comentario,r.FechaCreacion,r.RespuestaShaper,r.FechaRespuesta,r.Moderada,r.MotivoModeracion,p.Titulo ProductoTitulo,COALESCE(NULLIF(s.NombreDeNegosio,''),s.Nombre) ShaperNombre
            FROM ResenasProductos r INNER JOIN Usuarios u ON u.Id=r.ClienteId INNER JOIN Productos p ON p.Id=r.ProductoId INNER JOIN Usuarios s ON s.Id=p.ShaperId WHERE r.ProductoId=@ProductoId AND r.Moderada=0
            ORDER BY r.FechaCreacion DESC;";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn);
        cmd.Parameters.Add("@ProductoId",SqlDbType.Int).Value=productoId; cn.Open(); using var rd=cmd.ExecuteReader();
        var lista=new List<ResenaProducto>(); while(rd.Read()) lista.Add(MapearResena(rd)); return lista;
    }

    public Dictionary<int, ResumenResenas> ObtenerResumenes(IEnumerable<int> productoIds)
    {
        var ids=productoIds.Distinct().ToArray(); var resultado=new Dictionary<int,ResumenResenas>(); if(ids.Length==0)return resultado;
        var nombres=ids.Select((_,i)=>"@Id"+i).ToArray();
        string sql=$"SELECT ProductoId,AVG(CAST(Estrellas AS FLOAT)) Promedio,COUNT(*) Cantidad FROM ResenasProductos WHERE Moderada=0 AND ProductoId IN ({string.Join(",",nombres)}) GROUP BY ProductoId;";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn);
        for(int i=0;i<ids.Length;i++)cmd.Parameters.Add(nombres[i],SqlDbType.Int).Value=ids[i]; cn.Open(); using var rd=cmd.ExecuteReader();
        while(rd.Read())resultado[Convert.ToInt32(rd["ProductoId"])]=new(Convert.ToDouble(rd["Promedio"]),Convert.ToInt32(rd["Cantidad"])); return resultado;
    }

    public bool AlternarFavorito(int clienteId,int productoId)
    {
        const string sql=@"IF EXISTS(SELECT 1 FROM FavoritosProductos WHERE ClienteId=@ClienteId AND ProductoId=@ProductoId)
            BEGIN DELETE FROM FavoritosProductos WHERE ClienteId=@ClienteId AND ProductoId=@ProductoId; SELECT 0; END
            ELSE IF EXISTS(SELECT 1 FROM Productos WHERE Id=@ProductoId AND DELETED=0)
            BEGIN INSERT INTO FavoritosProductos(ClienteId,ProductoId,PrecioGuardado) SELECT @ClienteId,Id,Precio FROM Productos WHERE Id=@ProductoId; SELECT 1; END
            ELSE SELECT 0;";
        using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);AgregarClienteProducto(cmd,clienteId,productoId);cn.Open();return Convert.ToInt32(cmd.ExecuteScalar())==1;
    }
    public bool EsFavorito(int clienteId,int productoId){const string sql="SELECT COUNT(*) FROM FavoritosProductos WHERE ClienteId=@ClienteId AND ProductoId=@ProductoId";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);AgregarClienteProducto(cmd,clienteId,productoId);cn.Open();return Convert.ToInt32(cmd.ExecuteScalar())>0;}
    public List<FavoritoProducto> ObtenerFavoritos(int clienteId){const string sql=@"SELECT p.Id ProductoId,p.ShaperId,p.Titulo,p.Subtitulo,p.ImagenUrl,p.Precio,f.PrecioGuardado,p.TipoProducto,f.FechaCreacion,CASE WHEN p.DELETED=1 THEN CAST(0 AS BIT) WHEN p.TipoProducto='Tabla' THEN ISNULL(t.Disponible,0) WHEN p.TipoProducto='Leash' THEN IIF(ISNULL(l.Stock,0)>0,1,0) WHEN p.TipoProducto='Pad' THEN IIF(ISNULL(pa.Stock,0)>0,1,0) WHEN p.TipoProducto='Quilla' THEN IIF(ISNULL(q.Stock,0)>0,1,0) WHEN p.TipoProducto='Traje' THEN IIF(ISNULL(tr.Stock,0)>0,1,0) ELSE CAST(0 AS BIT) END Disponible,CASE p.TipoProducto WHEN 'Leash' THEN l.Stock WHEN 'Pad' THEN pa.Stock WHEN 'Quilla' THEN q.Stock WHEN 'Traje' THEN tr.Stock ELSE NULL END Stock FROM FavoritosProductos f INNER JOIN Productos p ON p.Id=f.ProductoId LEFT JOIN Tablas t ON t.ProductoId=p.Id LEFT JOIN Leashes l ON l.ProductoId=p.Id LEFT JOIN Pads pa ON pa.ProductoId=p.Id LEFT JOIN Quillas q ON q.ProductoId=p.Id LEFT JOIN Trajes tr ON tr.ProductoId=p.Id WHERE f.ClienteId=@ClienteId ORDER BY f.FechaCreacion DESC";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cn.Open();using var rd=cmd.ExecuteReader();var lista=new List<FavoritoProducto>();while(rd.Read())lista.Add(new(){ProductoId=Convert.ToInt32(rd["ProductoId"]),ShaperId=Convert.ToInt32(rd["ShaperId"]),Titulo=Convert.ToString(rd["Titulo"])??"",Subtitulo=Convert.ToString(rd["Subtitulo"])??"",ImagenUrl=Convert.ToString(rd["ImagenUrl"])??"",Precio=Convert.ToDecimal(rd["Precio"]),PrecioGuardado=Convert.ToDecimal(rd["PrecioGuardado"]),Disponible=Convert.ToBoolean(rd["Disponible"]),Stock=rd["Stock"]==DBNull.Value?null:Convert.ToInt32(rd["Stock"]),TipoProducto=Convert.ToString(rd["TipoProducto"])??"",FechaCreacion=Convert.ToDateTime(rd["FechaCreacion"])});return lista;}
    public int EliminarFavoritos(int clienteId,IEnumerable<int> productoIds)=>EliminarVarios("FavoritosProductos","ProductoId",clienteId,productoIds);

    public int GuardarDiseno(DisenoGuardado d){const string sql=@"INSERT INTO DisenosGuardados(ClienteId,ShaperId,Nombre,ConfiguracionJson) OUTPUT INSERTED.Id VALUES(@ClienteId,@ShaperId,@Nombre,@Json)";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=d.ClienteId;cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=d.ShaperId;cmd.Parameters.Add("@Nombre",SqlDbType.NVarChar,100).Value=d.Nombre;cmd.Parameters.Add("@Json",SqlDbType.NVarChar,-1).Value=d.ConfiguracionJson;cn.Open();return Convert.ToInt32(cmd.ExecuteScalar());}
    public List<DisenoGuardado> ObtenerDisenos(int clienteId)=>ObtenerDisenosInterno("ClienteId=@ClienteId",clienteId);
    public DisenoGuardado? ObtenerDiseno(int id,int clienteId)=>ObtenerDisenosInterno("Id=@Id AND ClienteId=@ClienteId",clienteId,id).SingleOrDefault();
    public bool EliminarDiseno(int id,int clienteId){const string sql="DELETE FROM DisenosGuardados WHERE Id=@Id AND ClienteId=@ClienteId";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cn.Open();return cmd.ExecuteNonQuery()==1;}
    public int EliminarDisenos(int clienteId,IEnumerable<int> ids)=>EliminarVarios("DisenosGuardados","Id",clienteId,ids);
    public bool RenombrarDiseno(int id,int clienteId,string nombre){const string sql="UPDATE DisenosGuardados SET Nombre=@Nombre,FechaActualizacion=SYSUTCDATETIME() WHERE Id=@Id AND ClienteId=@ClienteId";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Nombre",SqlDbType.NVarChar,100).Value=nombre;cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cn.Open();return cmd.ExecuteNonQuery()==1;}
    public int? DuplicarDiseno(int id,int clienteId,string nombre){const string sql=@"INSERT INTO DisenosGuardados(ClienteId,ShaperId,Nombre,ConfiguracionJson) OUTPUT INSERTED.Id SELECT ClienteId,ShaperId,@Nombre,ConfiguracionJson FROM DisenosGuardados WHERE Id=@Id AND ClienteId=@ClienteId";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Nombre",SqlDbType.NVarChar,100).Value=nombre;cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cn.Open();object? valor=cmd.ExecuteScalar();return valor==null?null:Convert.ToInt32(valor);}
    private static List<DisenoGuardado> ObtenerDisenosInterno(string filtro,int clienteId,int? id=null){string sql=$"SELECT Id,ClienteId,ShaperId,Nombre,ConfiguracionJson,FechaCreacion,FechaActualizacion FROM DisenosGuardados WHERE {filtro} ORDER BY COALESCE(FechaActualizacion,FechaCreacion) DESC";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;if(id.HasValue)cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id.Value;cn.Open();using var rd=cmd.ExecuteReader();var l=new List<DisenoGuardado>();while(rd.Read())l.Add(new(){Id=Convert.ToInt32(rd["Id"]),ClienteId=Convert.ToInt32(rd["ClienteId"]),ShaperId=Convert.ToInt32(rd["ShaperId"]),Nombre=Convert.ToString(rd["Nombre"])??"",ConfiguracionJson=Convert.ToString(rd["ConfiguracionJson"])??"{}",FechaCreacion=Convert.ToDateTime(rd["FechaCreacion"]),FechaActualizacion=rd["FechaActualizacion"]==DBNull.Value?null:Convert.ToDateTime(rd["FechaActualizacion"])});return l;}
    private static int EliminarVarios(string tabla,string columna,int clienteId,IEnumerable<int> valores){int[] ids=valores.Where(x=>x>0).Distinct().Take(200).ToArray();if(ids.Length==0)return 0;string[] nombres=ids.Select((_,i)=>"@Id"+i).ToArray();string sql=$"DELETE FROM {tabla} WHERE ClienteId=@ClienteId AND {columna} IN ({string.Join(',',nombres)})";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;for(int i=0;i<ids.Length;i++)cmd.Parameters.Add(nombres[i],SqlDbType.Int).Value=ids[i];cn.Open();return cmd.ExecuteNonQuery();}
    private static void AgregarClienteProducto(SqlCommand c,int clienteId,int productoId){c.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;c.Parameters.Add("@ProductoId",SqlDbType.Int).Value=productoId;}
    private static void ParametrosCompra(SqlCommand c,int clienteId,int pedidoId,int productoId){AgregarClienteProducto(c,clienteId,productoId);c.Parameters.Add("@PedidoId",SqlDbType.Int).Value=pedidoId;}
    public bool EditarResena(int id,int clienteId,byte estrellas,string comentario){const string sql="UPDATE ResenasProductos SET Estrellas=@Estrellas,Comentario=@Comentario,FechaEdicion=SYSUTCDATETIME() WHERE Id=@Id AND ClienteId=@ClienteId AND Moderada=0";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cmd.Parameters.Add("@Estrellas",SqlDbType.TinyInt).Value=estrellas;cmd.Parameters.Add("@Comentario",SqlDbType.NVarChar,1000).Value=comentario;cn.Open();return cmd.ExecuteNonQuery()==1;}
    public bool ResponderResena(int id,int shaperId,string respuesta){const string sql=@"UPDATE r SET RespuestaShaper=@Respuesta,FechaRespuesta=SYSUTCDATETIME() FROM ResenasProductos r INNER JOIN Productos p ON p.Id=r.ProductoId WHERE r.Id=@Id AND p.ShaperId=@ShaperId";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId;cmd.Parameters.Add("@Respuesta",SqlDbType.NVarChar,1000).Value=respuesta;cn.Open();return cmd.ExecuteNonQuery()==1;}
    public bool ModerarResena(int id,bool moderada,string motivo){const string sql="UPDATE ResenasProductos SET Moderada=@Moderada,MotivoModeracion=@Motivo WHERE Id=@Id";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id;cmd.Parameters.Add("@Moderada",SqlDbType.Bit).Value=moderada;cmd.Parameters.Add("@Motivo",SqlDbType.NVarChar,300).Value=motivo??"";cn.Open();return cmd.ExecuteNonQuery()==1;}
    public ResenaProducto? ObtenerResena(int id)=>ObtenerResenasAdministracion("","").FirstOrDefault(x=>x.Id==id);
    public List<ResenaProducto> ObtenerResenasAdministracion(string busqueda,string estado){const string sql=@"SELECT r.Id,r.ProductoId,r.ClienteId,u.Nombre ClienteNombre,r.Estrellas,r.Comentario,r.FechaCreacion,r.RespuestaShaper,r.FechaRespuesta,r.Moderada,r.MotivoModeracion,p.Titulo ProductoTitulo,COALESCE(NULLIF(s.NombreDeNegosio,''),s.Nombre) ShaperNombre FROM ResenasProductos r INNER JOIN Usuarios u ON u.Id=r.ClienteId INNER JOIN Productos p ON p.Id=r.ProductoId INNER JOIN Usuarios s ON s.Id=p.ShaperId WHERE (@Busqueda='' OR u.Nombre LIKE '%'+@Busqueda+'%' OR p.Titulo LIKE '%'+@Busqueda+'%' OR r.Comentario LIKE '%'+@Busqueda+'%') AND (@Estado='' OR (@Estado='visibles' AND r.Moderada=0) OR (@Estado='ocultas' AND r.Moderada=1)) ORDER BY r.FechaCreacion DESC";using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@Busqueda",SqlDbType.NVarChar,150).Value=busqueda??"";cmd.Parameters.Add("@Estado",SqlDbType.NVarChar,20).Value=estado??"";cn.Open();using var rd=cmd.ExecuteReader();var l=new List<ResenaProducto>();while(rd.Read())l.Add(MapearResena(rd));return l;}
    private static ResenaProducto MapearResena(SqlDataReader r)=>new(){Id=Convert.ToInt32(r["Id"]),ProductoId=Convert.ToInt32(r["ProductoId"]),ClienteId=Convert.ToInt32(r["ClienteId"]),ClienteNombre=Convert.ToString(r["ClienteNombre"])??"",Estrellas=Convert.ToByte(r["Estrellas"]),Comentario=Convert.ToString(r["Comentario"])??"",FechaCreacion=Convert.ToDateTime(r["FechaCreacion"]),RespuestaShaper=Convert.ToString(r["RespuestaShaper"])??"",FechaRespuesta=r["FechaRespuesta"]==DBNull.Value?null:Convert.ToDateTime(r["FechaRespuesta"]),Moderada=Convert.ToBoolean(r["Moderada"]),MotivoModeracion=Convert.ToString(r["MotivoModeracion"])??"",ProductoTitulo=Convert.ToString(r["ProductoTitulo"])??"",ShaperNombre=Convert.ToString(r["ShaperNombre"])??""};
}
