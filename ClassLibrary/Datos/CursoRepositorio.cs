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
    bool CrearInscripcion(int cursoId, int clienteId, int? moduloId, out string mensaje);
    List<InscripcionCurso> ObtenerInscripcionesDelShaper(int shaperId);
}

public class CursoRepositorio : ICursoRepositorio
{
    private const string Seleccion = @"
        SELECT c.Id,c.ShaperId,c.Titulo,c.Resumen,c.Descripcion,c.Modalidad,c.Ubicacion,
               c.FechaInicio,c.DuracionHoras,c.Cupos,c.Precio,c.ImagenUrl,c.InstructorNombre,
               c.InstructorBio,c.PublicoObjetivo,c.Incluye,c.Requisitos,c.CronogramaUrl,c.Contacto,
               c.CuotasMaximas,c.DescuentoAcompanantePorcentaje,c.HospedajeDisponible,c.Publicado,
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
        if(!rd.Read()) return null;
        var curso=Mapear(rd); rd.Close();
        CargarDetalles(cn,curso);
        return curso;
    }

    public int Insertar(CursoShaper c)
    {
        const string sql=@"INSERT INTO CursosShaper(ShaperId,Titulo,Resumen,Descripcion,Modalidad,Ubicacion,FechaInicio,DuracionHoras,Cupos,Precio,ImagenUrl,InstructorNombre,InstructorBio,PublicoObjetivo,Incluye,Requisitos,CronogramaUrl,Contacto,CuotasMaximas,DescuentoAcompanantePorcentaje,HospedajeDisponible,Publicado)
          OUTPUT INSERTED.Id VALUES(@ShaperId,@Titulo,@Resumen,@Descripcion,@Modalidad,@Ubicacion,@FechaInicio,@DuracionHoras,@Cupos,@Precio,@ImagenUrl,@InstructorNombre,@InstructorBio,@PublicoObjetivo,@Incluye,@Requisitos,@CronogramaUrl,@Contacto,@CuotasMaximas,@DescuentoAcompanantePorcentaje,@HospedajeDisponible,@Publicado);";
        using var cn=Conexion.ObtenerConexion(); cn.Open(); using var tx=cn.BeginTransaction();
        using var cmd=new SqlCommand(sql,cn,tx); ParametrosCurso(cmd,c); c.Id=Convert.ToInt32(cmd.ExecuteScalar()); GuardarDetalles(cn,tx,c); tx.Commit(); return c.Id;
    }

    public bool Actualizar(CursoShaper c,int shaperId)
    {
        const string sql=@"UPDATE CursosShaper SET Titulo=@Titulo,Resumen=@Resumen,Descripcion=@Descripcion,Modalidad=@Modalidad,Ubicacion=@Ubicacion,FechaInicio=@FechaInicio,DuracionHoras=@DuracionHoras,Cupos=@Cupos,Precio=@Precio,ImagenUrl=@ImagenUrl,InstructorNombre=@InstructorNombre,InstructorBio=@InstructorBio,PublicoObjetivo=@PublicoObjetivo,Incluye=@Incluye,Requisitos=@Requisitos,CronogramaUrl=@CronogramaUrl,Contacto=@Contacto,CuotasMaximas=@CuotasMaximas,DescuentoAcompanantePorcentaje=@DescuentoAcompanantePorcentaje,HospedajeDisponible=@HospedajeDisponible,Publicado=@Publicado,FechaActualizacion=SYSUTCDATETIME() WHERE Id=@Id AND ShaperId=@ShaperId;";
        using var cn=Conexion.ObtenerConexion(); cn.Open(); using var tx=cn.BeginTransaction(); using var cmd=new SqlCommand(sql,cn,tx); c.ShaperId=shaperId; ParametrosCurso(cmd,c); cmd.Parameters.Add("@Id",SqlDbType.Int).Value=c.Id;
        if(cmd.ExecuteNonQuery()!=1){tx.Rollback();return false;} GuardarDetalles(cn,tx,c); tx.Commit(); return true;
    }

    public bool CambiarPublicacion(int id,int shaperId,bool publicado)
    {
        using var cn=Conexion.ObtenerConexion(); using var cmd=new SqlCommand("UPDATE CursosShaper SET Publicado=@Publicado,FechaActualizacion=SYSUTCDATETIME() WHERE Id=@Id AND ShaperId=@ShaperId;",cn);
        cmd.Parameters.Add("@Publicado",SqlDbType.Bit).Value=publicado; cmd.Parameters.Add("@Id",SqlDbType.Int).Value=id; cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId; cn.Open(); return cmd.ExecuteNonQuery()==1;
    }

    public bool CrearInscripcion(int cursoId,int clienteId,int? moduloId,out string mensaje)
    {
        using var cn=Conexion.ObtenerConexion(); cn.Open(); using var tx=cn.BeginTransaction(IsolationLevel.Serializable);
        try {
            const string consulta=@"SELECT c.Precio,c.Cupos,c.Publicado,c.FechaInicio,(SELECT COUNT(*) FROM InscripcionesCursos i WHERE i.CursoId=c.Id AND i.Estado IN(0,1)) Ocupados FROM CursosShaper c WITH(UPDLOCK,HOLDLOCK) WHERE c.Id=@CursoId;";
            using var buscar=new SqlCommand(consulta,cn,tx); buscar.Parameters.Add("@CursoId",SqlDbType.Int).Value=cursoId;
            decimal precio; int cupos,ocupados; bool publicado; DateTime fecha;
            using(var rd=buscar.ExecuteReader()){ if(!rd.Read()){mensaje="El curso no existe.";tx.Rollback();return false;} precio=Convert.ToDecimal(rd["Precio"]);cupos=Convert.ToInt32(rd["Cupos"]);ocupados=Convert.ToInt32(rd["Ocupados"]);publicado=Convert.ToBoolean(rd["Publicado"]);fecha=Convert.ToDateTime(rd["FechaInicio"]); }
            if(!publicado||fecha<=DateTime.UtcNow){mensaje="El curso ya no admite inscripciones.";tx.Rollback();return false;}
            if(ocupados>=cupos){mensaje="No quedan cupos disponibles.";tx.Rollback();return false;}
            if(moduloId.HasValue){using var modulo=new SqlCommand("SELECT Precio FROM CursoModulos WHERE Id=@ModuloId AND CursoId=@CursoId AND PermiteCompraIndividual=1;",cn,tx);modulo.Parameters.Add("@ModuloId",SqlDbType.Int).Value=moduloId.Value;modulo.Parameters.Add("@CursoId",SqlDbType.Int).Value=cursoId;var valor=modulo.ExecuteScalar();if(valor==null||valor==DBNull.Value){mensaje="El módulo elegido no está disponible para compra individual.";tx.Rollback();return false;}precio=Convert.ToDecimal(valor);}
            decimal tasa=ObtenerTasaComision();
            decimal porcentaje=tasa*100m;
            decimal comision=Math.Round(precio*tasa,2,MidpointRounding.AwayFromZero);
            const string insertar=@"INSERT INTO InscripcionesCursos(CursoId,ClienteId,ModuloId,PrecioSnapshot,PorcentajeComision,ComisionPlataforma,NetoShaper,Estado) VALUES(@CursoId,@ClienteId,@ModuloId,@Precio,@Porcentaje,@Comision,@Neto,0);";
            using var cmd=new SqlCommand(insertar,cn,tx); cmd.Parameters.Add("@CursoId",SqlDbType.Int).Value=cursoId;cmd.Parameters.Add("@ClienteId",SqlDbType.Int).Value=clienteId;cmd.Parameters.Add("@ModuloId",SqlDbType.Int).Value=(object?)moduloId??DBNull.Value;cmd.Parameters.Add("@Precio",SqlDbType.Decimal).Value=precio;cmd.Parameters.Add("@Porcentaje",SqlDbType.Decimal).Value=porcentaje;cmd.Parameters.Add("@Comision",SqlDbType.Decimal).Value=comision;cmd.Parameters.Add("@Neto",SqlDbType.Decimal).Value=precio-comision;cmd.ExecuteNonQuery();tx.Commit();mensaje="Reservamos tu lugar. El pago se habilitará cuando se conecte el checkout de cursos.";return true;
        } catch(SqlException ex) when(ex.Number is 2601 or 2627){tx.Rollback();mensaje="Ya tenés una inscripción para este curso.";return false;}
    }

    public List<InscripcionCurso> ObtenerInscripcionesDelShaper(int shaperId)
    {
        var lista=new List<InscripcionCurso>(); const string sql=@"SELECT i.Id,i.CursoId,i.ClienteId,i.ModuloId,COALESCE(m.Nombre,N'Workshop completo') OpcionElegida,u.Nombre,u.Email,i.PrecioSnapshot,i.PorcentajeComision,i.ComisionPlataforma,i.NetoShaper,i.Estado,i.FechaCreacion FROM InscripcionesCursos i INNER JOIN CursosShaper c ON c.Id=i.CursoId LEFT JOIN CursoModulos m ON m.Id=i.ModuloId INNER JOIN Usuarios u ON u.Id=i.ClienteId WHERE c.ShaperId=@ShaperId ORDER BY i.FechaCreacion DESC;";
        using var cn=Conexion.ObtenerConexion();using var cmd=new SqlCommand(sql,cn);cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=shaperId;cn.Open();using var rd=cmd.ExecuteReader();while(rd.Read())lista.Add(new InscripcionCurso{Id=Convert.ToInt32(rd["Id"]),CursoId=Convert.ToInt32(rd["CursoId"]),ClienteId=Convert.ToInt32(rd["ClienteId"]),ModuloId=rd["ModuloId"]==DBNull.Value?null:Convert.ToInt32(rd["ModuloId"]),OpcionElegida=Convert.ToString(rd["OpcionElegida"])??"Workshop completo",ClienteNombre=Convert.ToString(rd["Nombre"])??"",ClienteEmail=Convert.ToString(rd["Email"])??"",Precio=Convert.ToDecimal(rd["PrecioSnapshot"]),PorcentajeComision=Convert.ToDecimal(rd["PorcentajeComision"]),ComisionPlataforma=Convert.ToDecimal(rd["ComisionPlataforma"]),NetoShaper=Convert.ToDecimal(rd["NetoShaper"]),Estado=Convert.ToByte(rd["Estado"]),FechaCreacion=Convert.ToDateTime(rd["FechaCreacion"])});return lista;
    }

    private static void CargarDetalles(SqlConnection cn,CursoShaper curso)
    {
        using(var cmd=new SqlCommand("SELECT Id,CursoId,Nombre,Descripcion,Encuentros,DuracionHoras,Precio,PermiteCompraIndividual,Orden FROM CursoModulos WHERE CursoId=@CursoId ORDER BY Orden,Id;",cn)){cmd.Parameters.Add("@CursoId",SqlDbType.Int).Value=curso.Id;using var rd=cmd.ExecuteReader();while(rd.Read())curso.Modulos.Add(new CursoModulo{Id=Convert.ToInt32(rd["Id"]),CursoId=curso.Id,Nombre=Convert.ToString(rd["Nombre"])??"",Descripcion=Convert.ToString(rd["Descripcion"])??"",Encuentros=Convert.ToInt32(rd["Encuentros"]),DuracionHoras=Convert.ToDecimal(rd["DuracionHoras"]),Precio=rd["Precio"]==DBNull.Value?null:Convert.ToDecimal(rd["Precio"]),PermiteCompraIndividual=Convert.ToBoolean(rd["PermiteCompraIndividual"]),Orden=Convert.ToInt32(rd["Orden"])});}
        using(var cmd=new SqlCommand("SELECT ImagenUrl FROM CursoImagenes WHERE CursoId=@CursoId ORDER BY Orden,Id;",cn)){cmd.Parameters.Add("@CursoId",SqlDbType.Int).Value=curso.Id;using var rd=cmd.ExecuteReader();while(rd.Read())curso.Imagenes.Add(Convert.ToString(rd["ImagenUrl"])??"");}
    }

    private static void GuardarDetalles(SqlConnection cn,SqlTransaction tx,CursoShaper curso)
    {
        var ids=curso.Modulos.Where(x=>x.Id>0).Select(x=>x.Id).ToList();
        string filtro=ids.Count==0?"":" AND Id NOT IN ("+string.Join(",",ids)+")";
        using(var borrar=new SqlCommand("DELETE FROM CursoModulos WHERE CursoId=@CursoId"+filtro+" AND NOT EXISTS(SELECT 1 FROM InscripcionesCursos i WHERE i.ModuloId=CursoModulos.Id);",cn,tx)){borrar.Parameters.Add("@CursoId",SqlDbType.Int).Value=curso.Id;borrar.ExecuteNonQuery();}
        for(int i=0;i<curso.Modulos.Count;i++){
            var m=curso.Modulos[i];
            string sql=m.Id>0?"UPDATE CursoModulos SET Nombre=@Nombre,Descripcion=@Descripcion,Encuentros=@Encuentros,DuracionHoras=@DuracionHoras,Precio=@Precio,PermiteCompraIndividual=@Permite,Orden=@Orden WHERE Id=@Id AND CursoId=@CursoId;":"INSERT INTO CursoModulos(CursoId,Nombre,Descripcion,Encuentros,DuracionHoras,Precio,PermiteCompraIndividual,Orden) VALUES(@CursoId,@Nombre,@Descripcion,@Encuentros,@DuracionHoras,@Precio,@Permite,@Orden);";
            using var cmd=new SqlCommand(sql,cn,tx);cmd.Parameters.Add("@CursoId",SqlDbType.Int).Value=curso.Id;if(m.Id>0)cmd.Parameters.Add("@Id",SqlDbType.Int).Value=m.Id;cmd.Parameters.Add("@Nombre",SqlDbType.NVarChar,120).Value=m.Nombre;cmd.Parameters.Add("@Descripcion",SqlDbType.NVarChar,1000).Value=m.Descripcion;cmd.Parameters.Add("@Encuentros",SqlDbType.Int).Value=m.Encuentros;cmd.Parameters.Add("@DuracionHoras",SqlDbType.Decimal).Value=m.DuracionHoras;cmd.Parameters.Add("@Precio",SqlDbType.Decimal).Value=(object?)m.Precio??DBNull.Value;cmd.Parameters.Add("@Permite",SqlDbType.Bit).Value=m.PermiteCompraIndividual;cmd.Parameters.Add("@Orden",SqlDbType.Int).Value=i;cmd.ExecuteNonQuery();
        }
        using(var borrar=new SqlCommand("DELETE FROM CursoImagenes WHERE CursoId=@CursoId;",cn,tx)){borrar.Parameters.Add("@CursoId",SqlDbType.Int).Value=curso.Id;borrar.ExecuteNonQuery();}
        for(int i=0;i<curso.Imagenes.Count;i++){using var cmd=new SqlCommand("INSERT INTO CursoImagenes(CursoId,ImagenUrl,Orden) VALUES(@CursoId,@ImagenUrl,@Orden);",cn,tx);cmd.Parameters.Add("@CursoId",SqlDbType.Int).Value=curso.Id;cmd.Parameters.Add("@ImagenUrl",SqlDbType.NVarChar,600).Value=curso.Imagenes[i];cmd.Parameters.Add("@Orden",SqlDbType.Int).Value=i;cmd.ExecuteNonQuery();}
    }

    private static void ParametrosCurso(SqlCommand cmd,CursoShaper c){cmd.Parameters.Add("@ShaperId",SqlDbType.Int).Value=c.ShaperId;cmd.Parameters.Add("@Titulo",SqlDbType.NVarChar,140).Value=c.Titulo;cmd.Parameters.Add("@Resumen",SqlDbType.NVarChar,280).Value=c.Resumen;cmd.Parameters.Add("@Descripcion",SqlDbType.NVarChar,-1).Value=c.Descripcion;cmd.Parameters.Add("@Modalidad",SqlDbType.NVarChar,30).Value=c.Modalidad;cmd.Parameters.Add("@Ubicacion",SqlDbType.NVarChar,180).Value=c.Ubicacion;cmd.Parameters.Add("@FechaInicio",SqlDbType.DateTime2).Value=c.FechaInicio;cmd.Parameters.Add("@DuracionHoras",SqlDbType.Decimal).Value=c.DuracionHoras;cmd.Parameters.Add("@Cupos",SqlDbType.Int).Value=c.Cupos;cmd.Parameters.Add("@Precio",SqlDbType.Decimal).Value=c.Precio;cmd.Parameters.Add("@ImagenUrl",SqlDbType.NVarChar,600).Value=(object?)c.ImagenUrl??DBNull.Value;cmd.Parameters.Add("@InstructorNombre",SqlDbType.NVarChar,140).Value=c.InstructorNombre;cmd.Parameters.Add("@InstructorBio",SqlDbType.NVarChar,1800).Value=c.InstructorBio;cmd.Parameters.Add("@PublicoObjetivo",SqlDbType.NVarChar,2500).Value=c.PublicoObjetivo;cmd.Parameters.Add("@Incluye",SqlDbType.NVarChar,3000).Value=c.Incluye;cmd.Parameters.Add("@Requisitos",SqlDbType.NVarChar,1800).Value=c.Requisitos;cmd.Parameters.Add("@CronogramaUrl",SqlDbType.NVarChar,600).Value=(object?)c.CronogramaUrl??DBNull.Value;cmd.Parameters.Add("@Contacto",SqlDbType.NVarChar,500).Value=(object?)c.Contacto??DBNull.Value;cmd.Parameters.Add("@CuotasMaximas",SqlDbType.TinyInt).Value=c.CuotasMaximas;cmd.Parameters.Add("@DescuentoAcompanantePorcentaje",SqlDbType.Decimal).Value=c.DescuentoAcompanantePorcentaje;cmd.Parameters.Add("@HospedajeDisponible",SqlDbType.Bit).Value=c.HospedajeDisponible;cmd.Parameters.Add("@Publicado",SqlDbType.Bit).Value=c.Publicado;}
    private static decimal ObtenerTasaComision(){string? valor=Environment.GetEnvironmentVariable("MP_COMISION_PLATAFORMA");if(!decimal.TryParse(valor,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out decimal tasa)||tasa<0||tasa>1)throw new InvalidOperationException("MP_COMISION_PLATAFORMA debe ser un valor entre 0 y 1.");return tasa;}
    private static CursoShaper Mapear(SqlDataReader r)=>new(){Id=Convert.ToInt32(r["Id"]),ShaperId=Convert.ToInt32(r["ShaperId"]),Titulo=Convert.ToString(r["Titulo"])??"",Resumen=Convert.ToString(r["Resumen"])??"",Descripcion=Convert.ToString(r["Descripcion"])??"",Modalidad=Convert.ToString(r["Modalidad"])??"",Ubicacion=Convert.ToString(r["Ubicacion"])??"",FechaInicio=Convert.ToDateTime(r["FechaInicio"]),DuracionHoras=Convert.ToDecimal(r["DuracionHoras"]),Cupos=Convert.ToInt32(r["Cupos"]),CuposOcupados=Convert.ToInt32(r["CuposOcupados"]),Precio=Convert.ToDecimal(r["Precio"]),ImagenUrl=r["ImagenUrl"]==DBNull.Value?null:Convert.ToString(r["ImagenUrl"]),InstructorNombre=Convert.ToString(r["InstructorNombre"])??"",InstructorBio=Convert.ToString(r["InstructorBio"])??"",PublicoObjetivo=Convert.ToString(r["PublicoObjetivo"])??"",Incluye=Convert.ToString(r["Incluye"])??"",Requisitos=Convert.ToString(r["Requisitos"])??"",CronogramaUrl=r["CronogramaUrl"]==DBNull.Value?null:Convert.ToString(r["CronogramaUrl"]),Contacto=r["Contacto"]==DBNull.Value?null:Convert.ToString(r["Contacto"]),CuotasMaximas=Convert.ToByte(r["CuotasMaximas"]),DescuentoAcompanantePorcentaje=Convert.ToDecimal(r["DescuentoAcompanantePorcentaje"]),HospedajeDisponible=Convert.ToBoolean(r["HospedajeDisponible"]),Publicado=Convert.ToBoolean(r["Publicado"]),ShaperNombre=Convert.ToString(r["ShaperNombre"])??"",NegocioShaper=Convert.ToString(r["NombreDeNegosio"])??""};
}
