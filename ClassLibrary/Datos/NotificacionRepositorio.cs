using ClassLibrary.Notificaciones;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos;

public interface INotificacionRepositorio
{
    long Crear(int usuarioId,string titulo,string mensaje,string? url=null,string tipo="General");
    List<NotificacionUsuario> Obtener(int usuarioId,int cantidad=50);
    int ContarNoLeidas(int usuarioId);
    bool MarcarLeida(long id,int usuarioId);
    int MarcarTodasLeidas(int usuarioId);
}

public class NotificacionRepositorio : INotificacionRepositorio
{
    public long Crear(int usuarioId,string titulo,string mensaje,string? url=null,string tipo="General")
    { const string sql=@"INSERT INTO NotificacionesUsuarios(UsuarioId,Titulo,Mensaje,Url,Tipo) OUTPUT INSERTED.Id VALUES(@UsuarioId,@Titulo,@Mensaje,@Url,@Tipo)";using var cn=Conexion.ObtenerConexion();using var c=new SqlCommand(sql,cn);c.Parameters.Add("@UsuarioId",SqlDbType.Int).Value=usuarioId;c.Parameters.Add("@Titulo",SqlDbType.NVarChar,160).Value=(titulo??"")[..Math.Min((titulo??"").Length,160)];c.Parameters.Add("@Mensaje",SqlDbType.NVarChar,1000).Value=(mensaje??"")[..Math.Min((mensaje??"").Length,1000)];c.Parameters.Add("@Url",SqlDbType.NVarChar,500).Value=(object?)url??DBNull.Value;c.Parameters.Add("@Tipo",SqlDbType.NVarChar,40).Value=(tipo??"General")[..Math.Min((tipo??"General").Length,40)];cn.Open();return Convert.ToInt64(c.ExecuteScalar()); }
    public List<NotificacionUsuario> Obtener(int usuarioId,int cantidad=50)
    { const string sql=@"SELECT TOP(@Cantidad) Id,UsuarioId,Titulo,Mensaje,Url,Tipo,Leida,FechaCreacion FROM NotificacionesUsuarios WHERE UsuarioId=@UsuarioId ORDER BY FechaCreacion DESC,Id DESC";using var cn=Conexion.ObtenerConexion();using var c=new SqlCommand(sql,cn);c.Parameters.Add("@UsuarioId",SqlDbType.Int).Value=usuarioId;c.Parameters.Add("@Cantidad",SqlDbType.Int).Value=Math.Clamp(cantidad,1,100);cn.Open();using var r=c.ExecuteReader();var l=new List<NotificacionUsuario>();while(r.Read())l.Add(new(){Id=Convert.ToInt64(r["Id"]),UsuarioId=Convert.ToInt32(r["UsuarioId"]),Titulo=Convert.ToString(r["Titulo"])??"",Mensaje=Convert.ToString(r["Mensaje"])??"",Url=r["Url"]==DBNull.Value?null:Convert.ToString(r["Url"]),Tipo=Convert.ToString(r["Tipo"])??"General",Leida=Convert.ToBoolean(r["Leida"]),FechaCreacion=Convert.ToDateTime(r["FechaCreacion"])});return l; }
    public int ContarNoLeidas(int usuarioId){const string sql="SELECT COUNT(*) FROM NotificacionesUsuarios WHERE UsuarioId=@UsuarioId AND Leida=0";using var cn=Conexion.ObtenerConexion();using var c=new SqlCommand(sql,cn);c.Parameters.Add("@UsuarioId",SqlDbType.Int).Value=usuarioId;cn.Open();return Convert.ToInt32(c.ExecuteScalar());}
    public bool MarcarLeida(long id,int usuarioId){const string sql="UPDATE NotificacionesUsuarios SET Leida=1,FechaLectura=COALESCE(FechaLectura,SYSUTCDATETIME()) WHERE Id=@Id AND UsuarioId=@UsuarioId";using var cn=Conexion.ObtenerConexion();using var c=new SqlCommand(sql,cn);c.Parameters.Add("@Id",SqlDbType.BigInt).Value=id;c.Parameters.Add("@UsuarioId",SqlDbType.Int).Value=usuarioId;cn.Open();return c.ExecuteNonQuery()==1;}
    public int MarcarTodasLeidas(int usuarioId){const string sql="UPDATE NotificacionesUsuarios SET Leida=1,FechaLectura=COALESCE(FechaLectura,SYSUTCDATETIME()) WHERE UsuarioId=@UsuarioId AND Leida=0";using var cn=Conexion.ObtenerConexion();using var c=new SqlCommand(sql,cn);c.Parameters.Add("@UsuarioId",SqlDbType.Int).Value=usuarioId;cn.Open();return c.ExecuteNonQuery();}
}
