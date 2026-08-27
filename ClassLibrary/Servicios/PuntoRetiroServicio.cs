using ClassLibrary.Datos;
using ClassLibrary.PuntosRetiro;

namespace ClassLibrary.Servicios;

public interface IPuntoRetiroServicio
{
    List<PuntoRetiro> ObtenerPorShaper(int shaperId);
    List<PuntoRetiro> ObtenerActivos();
    PuntoRetiro? ObtenerParaEditar(int id,int shaperId);
    (bool Exito,string Error) Guardar(PuntoRetiro punto,int shaperId);
    bool CambiarEstado(int id,int shaperId,bool activo);
}
public class PuntoRetiroServicio : IPuntoRetiroServicio
{
    private readonly IPuntoRetiroRepositorio _repositorio;
    public PuntoRetiroServicio(IPuntoRetiroRepositorio repositorio)=>_repositorio=repositorio;
    public List<PuntoRetiro> ObtenerPorShaper(int id)=>_repositorio.ObtenerPorShaper(id);
    public List<PuntoRetiro> ObtenerActivos()=>_repositorio.ObtenerActivos();
    public PuntoRetiro? ObtenerParaEditar(int id,int shaperId){var p=_repositorio.ObtenerPorId(id);return p?.ShaperId==shaperId?p:null;}
    public (bool Exito,string Error) Guardar(PuntoRetiro p,int shaperId)
    {
        p.ShaperId=shaperId; p.Nombre=Limitar(p.Nombre,150);p.Direccion=Limitar(p.Direccion,250);p.Ciudad=Limitar(p.Ciudad,120);
        p.Horario=Limitar(p.Horario,250);p.Indicaciones=Limitar(p.Indicaciones,500);
        if(string.IsNullOrWhiteSpace(p.Nombre)||string.IsNullOrWhiteSpace(p.Direccion)||string.IsNullOrWhiteSpace(p.Ciudad))return(false,"Completá el nombre, la dirección y la ciudad.");
        if(p.Latitud < -90 || p.Latitud > 90 || p.Longitud < -180 || p.Longitud > 180 || (p.Latitud==0 && p.Longitud==0))return(false,"Marcá la ubicación exacta del punto de retiro en el mapa.");
        if(p.Id>0 && ObtenerParaEditar(p.Id,shaperId)==null)return(false,"No se encontró el punto de retiro.");
        bool ok=p.Id>0?_repositorio.Actualizar(p):_repositorio.Insertar(p)>0;
        return ok?(true,string.Empty):(false,"No se pudo guardar el punto de retiro.");
    }
    public bool CambiarEstado(int id,int shaperId,bool activo)=>ObtenerParaEditar(id,shaperId)!=null&&_repositorio.CambiarEstado(id,shaperId,activo);
    private static string Limitar(string? s,int n){s=s?.Trim()??"";return s[..Math.Min(s.Length,n)];}
}
