using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Controllers;

namespace Pruebas;

public class CursosShaperTests
{
    [Theory]
    [InlineData(nameof(CursosController.Crear),"Shaper")]
    [InlineData(nameof(CursosController.Editar),"Shaper")]
    [InlineData(nameof(CursosController.CambiarPublicacion),"Shaper")]
    [InlineData(nameof(CursosController.Inscribirse),"Cliente")]
    public void OperacionesDeCursosExigenRolYProteccionCsrf(string accion,string rol)
    {
        MethodInfo metodo=typeof(CursosController).GetMethods()
            .Single(m=>m.Name==accion&&m.GetCustomAttribute<HttpPostAttribute>()!=null);
        Assert.Equal(rol,metodo.GetCustomAttribute<AuthorizeAttribute>()?.Roles);
        Assert.NotNull(metodo.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    [Fact]
    public void SqlRegistraPrecioComisionYNetoComoSnapshot()
    {
        string raiz=BuscarRaiz();
        string sql=File.ReadAllText(Path.Combine(raiz,"Database","AgregarCursosShaper.sql"));
        Assert.Contains("PrecioSnapshot",sql);
        Assert.Contains("PorcentajeComision",sql);
        Assert.Contains("ComisionPlataforma",sql);
        Assert.Contains("NetoShaper",sql);
        Assert.Contains("UNIQUE(CursoId,ClienteId)",sql.Replace(" ",""));
    }

    [Fact]
    public void CursosReutilizanLaMismaComisionDelCatalogo()
    {
        string repositorio=File.ReadAllText(Path.Combine(BuscarRaiz(),"ClassLibrary","Datos","CursoRepositorio.cs"));
        string configuracion=File.ReadAllText(Path.Combine(BuscarRaiz(),"WebApplication2",".env.example"));
        Assert.Contains("MP_COMISION_PLATAFORMA",repositorio);
        Assert.Contains("MP_COMISION_PLATAFORMA",configuracion);
        Assert.DoesNotContain("Cursos:PorcentajeComision",repositorio);
    }

    [Fact]
    public void PerfilPublicoIncluyeSeccionDeCursos()
    {
        string vista=File.ReadAllText(Path.Combine(BuscarRaiz(),"WebApplication2","Views","Shaper","Detalle.cshtml"));
        Assert.Contains("Model.Cursos",vista);
        Assert.Contains("Cursos y workshops",vista);
    }

    private static string BuscarRaiz(){var d=new DirectoryInfo(AppContext.BaseDirectory);while(d!=null&&!Directory.Exists(Path.Combine(d.FullName,"WebApplication2")))d=d.Parent;return d?.FullName??throw new DirectoryNotFoundException();}
}
