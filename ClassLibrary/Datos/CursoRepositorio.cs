using System.Data;
using System.Data.SqlClient;
using ClassLibrary.Cursos;

namespace ClassLibrary.Datos;

public interface ICursoRepositorio
{
    List<CursoShaper> ObtenerPorShaper(int shaperId, bool soloPublicados);
    CursoShaper? ObtenerPorId(int id);
    int Insertar(CursoShaper curso);
    bool Actualizar(CursoShaper curso, int shaperId);
    bool CambiarPublicacion(int id, int shaperId, bool publicado);
    bool CrearInscripcion(int cursoId, int clienteId, out string mensaje);
    List<InscripcionCurso> ObtenerInscripcionesDelShaper(int shaperId);
}

public class CursoRepositorio : ICursoRepositorio
{
    private const string Seleccion = @"
        SELECT c.Id,c.ShaperId,c.Titulo,c.Resumen,c.Descripcion,c.Modalidad,c.Ubicacion,
               c.FechaInicio,c.DuracionHoras,c.Cupos,c.Precio,c.ImagenUrl,c.Publicado,
               u.Nombre AS ShaperNombre,u.NombreDeNegosio,
               (SELECT COUNT(*) FROM InscripcionesCursos i
                WHERE i.CursoId=c.Id AND i.Estado IN (0,1)) AS CuposOcupados
        FROM CursosShaper c INNER JOIN Usuarios u ON u.Id=c.ShaperId ";

    public List<CursoShaper> ObtenerPorShaper(int shaperId, bool soloPublicados)
    {
        var cursos = new List<CursoShaper>();
        string sql = Seleccion + " WHERE c.ShaperId=@ShaperId AND (@SoloPublicados=0 OR c.Publicado=1) ORDER BY c.FechaInicio,c.Id;";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn);
        cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId;
        cmd.Parameters.Add("@SoloPublicados",SqlDbType.Bit).Value=soloPublicados;
        cn.Open(); using var rd=cmd.ExecuteReader(); while(rd.Read()) cursos.Add(Mapear(rd));
        return cursos;
    }

    public CursoShaper? ObtenerPorId(int id)
    {
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(Seleccion+" WHERE c.Id=@Id;",cn);
        cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id; cn.Open(); using var rd=cmd.ExecuteReader();
        return rd.Read()?Mapear(rd):null;
    }

    public int Insertar(CursoShaper c)
    {
        const string sql=@"INSERT INTO CursosShaper(ShaperId,Titulo,Resumen,Descripcion,Modalidad,Ubicacion,FechaInicio,DuracionHoras,Cupos,Precio,ImagenUrl,Publicado)
          OUTPUT INSERTED.Id VALUES(@ShaperId,@Titulo,@Resumen,@Descripcion,@Modalidad,@Ubicacion,@FechaInicio,@DuracionHoras,@Cupos,@Precio,@ImagenUrl,@Publicado);";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn); ParametrosCurso(cmd,c); cn.Open(); return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Actualizar(CursoShaper c,int shaperId)
    {
        const string sql=@"UPDATE CursosShaper SET Titulo=@Titulo,Resumen=@Resumen,Descripcion=@Descripcion,Modalidad=@Modalidad,Ubicacion=@Ubicacion,FechaInicio=@FechaInicio,DuracionHoras=@DuracionHoras,Cupos=@Cupos,Precio=@Precio,ImagenUrl=@ImagenUrl,Publicado=@Publicado,FechaActualizacion=SYSUTCDATETIME() WHERE Id=@Id AND ShaperId=@ShaperId;";
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand(sql,cn); c.ShaperId=shaperId; ParametrosCurso(cmd,c); cmd.Parameters.Add("@Id",SqlDbType.Int).Value=c.Id; cn.Open(); return cmd.ExecuteNonQuery()==1;
    }

    public bool CambiarPublicacion(int id,int shaperId,bool publicado)
    {
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand("UPDATE CursosShaper SET Publicado=@Publicado,FechaActualizacion=SYSUTCDATETIME() WHERE Id=@Id AND ShaperId=@ShaperId;",cn);
        cmd.Parameters.Add("@Publicado",SqlDbType.Bit).Value=publicado; cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id; cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId; cn.Open(); return cmd.ExecuteNonQuery()==1;
    }

    public bool CrearInscripcion(int cursoId,int clienteId,out string mensaje)
    {
        using var cn=Conexion.ObtenerConexion(); cn.Open(); using var tx=cn.BeginTransaction(IsolationLevel.Serializable);
        try {
            const string consulta=@"SELECT c.Precio,c.Cupos,c.Publicado,c.FechaInicio,(SELECT COUNT(*) FROM InscripcionesCursos i WHERE i.CursoId=c.Id AND i.Estado IN(0,1)) Ocupados FROM CursosShaper c WITH(UPDLOCK,HOLDLOCK) WHERE c.Id=@CursoId;";
            using var buscar=new SqlCommand(consulta,cn,tx); buscar.Parameters.Add("@CursoId",SqlDbType.Int).Value=cursoId;
            decimal precio; int cupos,ocupados; bool publicado; DateTime fecha;
            using(var rd=buscar.ExecuteReader()){ if(!rd.Read()){mensaje="El curso no existe.";tx.Rollback();return false;} precio=Convert.ToDecimal(rd["Precio"]);cupos=Convert.ToInt32(rd["Cupos"]);ocupados=Convert.ToInt32(rd["Ocupados"]);publicado=Convert.ToBoolean(rd["Publicado"]);fecha=Convert.ToDateTime(rd["FechaInicio"]); }
            if(!publicado||fecha<=DateTime.UtcNow){mensaje="El curso ya no admite inscripciones.";tx.Rollback();return false;}
            if(ocupados>=cupos){mensaje="No quedan cupos disponibles.";tx.Rollback();return false;}
            decimal tasa=ObtenerTasaComision();
            decimal porcentaje=tasa*100m;
            decimal comision=Math.Round(precio*tasa,2,MidpointRounding.AwayFromZero);
            const string insertar=@"INSERT INTO InscripcionesCursos(CursoId,ClienteId,PrecioSnapshot,PorcentajeComision,ComisionPlataforma,NetoShaper,Estado) VALUES(@CursoId,@ClienteId,@Precio,@Porcentaje,@Comision,@Neto,0);";
            using var cmd=new SqlCommand(insertar,cn,tx); cmd.Parameters.Add("@CursoId",SqlDbType.Int).Value=cursoId;cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cmd.Parameters.Add("@Precio",SqlDbType.Decimal).Value=precio;cmd.Parameters.Add("@Porcentaje",SqlDbType.Decimal).Value=porcentaje;cmd.Parameters.Add("@Comision",SqlDbType.Decimal).Value=comision;cmd.Parameters.Add("@Neto",SqlDbType.Decimal).Value=precio-comision;cmd.ExecuteNonQuery();tx.Commit();mensaje="Reservamos tu lugar. El pago se habilitará cuando se conecte el checkout de cursos.";return true;
        } catch(SqlException ex) when(ex.Number is 2601 or 2627){tx.Rollback();mensaje="Ya tenés una inscripción para este curso.";return false;}
    }

    public List<InscripcionCurso> ObtenerInscripcionesDelShaper(int shaperId)
    {
        var lista=new List<InscripcionCurso>(); const string sql=@"SELECT i.Id,i.CursoId,i.ClienteId,u.Nombre,u.Email,i.PrecioSnapshot,i.PorcentajeComision,i.ComisionPlataforma,i.NetoShaper,i.Estado,i.FechaCreacion FROM InscripcionesCursos i INNER JOIN CursosShaper c ON c.Id=i.CursoId INNER JOIN Usuarios u ON u.Id=i.ClienteId WHERE c.ShaperId=@ShaperId ORDER BY i.FechaCreacion DESC;";
        using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId;cn.Open();using var rd=cmd.ExecuteReader();while(rd.Read())lista.Add(new InscripcionCurso{Id=Convert.ToInt32(rd["Id"]),CursoId=Convert.ToInt32(rd["CursoId"]),ClienteId=Convert.ToInt32(rd["ClienteId"]),ClienteNombre=Convert.ToString(rd["Nombre"])??"",ClienteEmail=Convert.ToString(rd["Email"])??"",Precio=Convert.ToDecimal(rd["PrecioSnapshot"]),PorcentajeComision=Convert.ToDecimal(rd["PorcentajeComision"]),ComisionPlataforma=Convert.ToDecimal(rd["ComisionPlataforma"]),NetoShaper=Convert.ToDecimal(rd["NetoShaper"]),Estado=Convert.ToByte(rd["Estado"]),FechaCreacion=Convert.ToDateTime(rd["FechaCreacion"])});return lista;
    }

    private static void ParametrosCurso(SqlCommand cmd,CursoShaper c){cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=c.ShaperId;cmd.Parameters.Add("@Titulo",SqlDbType.NVarChar,140).Value=c.Titulo;cmd.Parameters.Add("@Resumen",SqlDbType.NVarChar,280).Value=c.Resumen;cmd.Parameters.Add("@Descripcion",SqlDbType.NVarChar,-1).Value=c.Descripcion;cmd.Parameters.Add("@Modalidad",SqlDbType.NVarChar,30).Value=c.Modalidad;cmd.Parameters.Add("@Ubicacion",SqlDbType.NVarChar,180).Value=c.Ubicacion;cmd.Parameters.Add("@FechaInicio",SqlDbType.DateTime2).Value=c.FechaInicio;cmd.Parameters.Add("@DuracionHoras",SqlDbType.Decimal).Value=c.DuracionHoras;cmd.Parameters.Add("@Cupos",SqlDbType.Int).Value=c.Cupos;cmd.Parameters.Add("@Precio",SqlDbType.Decimal).Value=c.Precio;cmd.Parameters.Add("@ImagenUrl",SqlDbType.NVarChar,600).Value=(object?)c.ImagenUrl??DBNull.Value;cmd.Parameters.Add("@Publicado",SqlDbType.Bit).Value=c.Publicado;}
    private static decimal ObtenerTasaComision(){string? valor=Environment.GetEnvironmentVariable("MP_COMISION_PLATAFORMA");if(!decimal.TryParse(valor,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out decimal tasa)||tasa<0||tasa>1)throw new InvalidOperationException("MP_COMISION_PLATAFORMA debe ser un valor entre 0 y 1.");return tasa;}
    private static CursoShaper Mapear(SqlDataReader r)=>new(){Id=Convert.ToInt32(r["Id"]),ShaperId=Convert.ToInt32(r["ShaperId"]),Titulo=Convert.ToString(r["Titulo"])??"",Resumen=Convert.ToString(r["Resumen"])??"",Descripcion=Convert.ToString(r["Descripcion"])??"",Modalidad=Convert.ToString(r["Modalidad"])??"",Ubicacion=Convert.ToString(r["Ubicacion"])??"",FechaInicio=Convert.ToDateTime(r["FechaInicio"]),DuracionHoras=Convert.ToDecimal(r["DuracionHoras"]),Cupos=Convert.ToInt32(r["Cupos"]),CuposOcupados=Convert.ToInt32(r["CuposOcupados"]),Precio=Convert.ToDecimal(r["Precio"]),ImagenUrl=r["ImagenUrl"]==DBNull.Value?null:Convert.ToString(r["ImagenUrl"]),Publicado=Convert.ToBoolean(r["Publicado"]),ShaperNombre=Convert.ToString(r["ShaperNombre"])??"",NegocioShaper=Convert.ToString(r["NombreDeNegosio"])??""};
}
