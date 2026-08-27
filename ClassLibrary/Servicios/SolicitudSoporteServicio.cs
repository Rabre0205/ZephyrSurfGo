using ClassLibrary.Datos;
using ClassLibrary.Soporte;

namespace ClassLibrary.Servicios;

public interface ISolicitudSoporteServicio
{
    (bool Exito,string Error,int Id) Crear(int shaperId,string asunto,string mensaje);
    List<SolicitudSoporte> ObtenerPorShaper(int shaperId);
    List<SolicitudSoporte> ObtenerTodas();
    SolicitudSoporte? ObtenerParaShaper(int id,int shaperId);
    SolicitudSoporte? ObtenerPorId(int id);
    bool Responder(int id,string respuesta,bool cerrar);
}
public class SolicitudSoporteServicio : ISolicitudSoporteServicio
{
    private readonly ISolicitudSoporteRepositorio _repositorio;
    public SolicitudSoporteServicio(ISolicitudSoporteRepositorio repositorio)=>_repositorio=repositorio;
    public (bool Exito,string Error,int Id) Crear(int shaperId,string asunto,string mensaje)
    {
        asunto=(asunto??"").Trim();mensaje=(mensaje??"").Trim();
        if(asunto.Length<4||mensaje.Length<10)return(false,"Describí el asunto y el problema con un poco más de detalle.",0);
        asunto=asunto[..Math.Min(asunto.Length,150)];mensaje=mensaje[..Math.Min(mensaje.Length,2000)];
        int id=_repositorio.Insertar(new(){ShaperId=shaperId,Asunto=asunto,Mensaje=mensaje});return id>0?(true,"",id):(false,"No se pudo enviar la consulta.",0);
    }
    public List<SolicitudSoporte> ObtenerPorShaper(int id)=>_repositorio.ObtenerPorShaper(id);
    public List<SolicitudSoporte> ObtenerTodas()=>_repositorio.ObtenerTodas();
    public SolicitudSoporte? ObtenerParaShaper(int id,int shaperId){var s=_repositorio.ObtenerPorId(id);return s?.ShaperId==shaperId?s:null;}
    public SolicitudSoporte? ObtenerPorId(int id)=>_repositorio.ObtenerPorId(id);
    public bool Responder(int id,string respuesta,bool cerrar){respuesta=(respuesta??"").Trim();return respuesta.Length>=2&&_repositorio.Responder(id,respuesta[..Math.Min(respuesta.Length,2000)],cerrar?(byte)2:(byte)1);}
}
