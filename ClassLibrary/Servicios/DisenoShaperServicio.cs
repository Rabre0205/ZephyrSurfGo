using ClassLibrary.Datos;
using ClassLibrary.Disenos;

namespace ClassLibrary.Servicios;

public interface IDisenoShaperServicio
{
    List<DisenoShaper> ObtenerPorShaper(int shaperId,bool soloActivos=false);
    DisenoShaper? ObtenerParaEditar(int id,int shaperId);
    (bool Exito,string Error) Guardar(DisenoShaper diseno,int shaperId);
    bool CambiarEstado(int id,int shaperId,bool activo);
}
public class DisenoShaperServicio : IDisenoShaperServicio
{
    private readonly IDisenoShaperRepositorio _repositorio;
    public DisenoShaperServicio(IDisenoShaperRepositorio repositorio)=>_repositorio=repositorio;
    public List<DisenoShaper> ObtenerPorShaper(int id,bool activos=false)=>_repositorio.ObtenerPorShaper(id,activos);
    public DisenoShaper? ObtenerParaEditar(int id,int shaperId){var d=_repositorio.ObtenerPorId(id);return d?.ShaperId==shaperId?d:null;}
    public (bool Exito,string Error) Guardar(DisenoShaper d,int shaperId)
    {
        d.ShaperId=shaperId;d.Nombre=Limitar(d.Nombre,120);d.Descripcion=Limitar(d.Descripcion,600);d.ZonaAplicacion=Limitar(d.ZonaAplicacion,20);
        d.ColorPrimario=NormalizarColor(d.ColorPrimario,"#ffffff");d.ColorSecundario=NormalizarColor(d.ColorSecundario,"#111111");
        if(string.IsNullOrWhiteSpace(d.Nombre))return(false,"Ingresá un nombre para el diseño.");
        if(d.ZonaAplicacion is not ("Deck" or "Bottom" or "Ambos"))return(false,"Seleccioná dónde se aplica el diseño.");
        if(d.Recargo<0||d.Recargo>100000)return(false,"El recargo debe estar entre USD 0 y USD 100.000.");
        if(d.Id>0&&ObtenerParaEditar(d.Id,shaperId)==null)return(false,"No se encontró el diseño.");
        bool ok=d.Id>0?_repositorio.Actualizar(d):_repositorio.Insertar(d)>0;
        return ok?(true,string.Empty):(false,"No se pudo guardar el diseño.");
    }
    public bool CambiarEstado(int id,int shaperId,bool activo)=>ObtenerParaEditar(id,shaperId)!=null&&_repositorio.CambiarEstado(id,shaperId,activo);
    private static string Limitar(string? s,int n){s=s?.Trim()??"";return s[..Math.Min(s.Length,n)];}
    private static string NormalizarColor(string? color,string predeterminado)=>System.Text.RegularExpressions.Regex.IsMatch(color??"","^#[0-9a-fA-F]{6}$")?color!:predeterminado;
}
