using ClassLibrary.Enums;
using ClassLibrary.Persona;
using ClassLibrary.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Pruebas;

public class LoginGoogleControllerTests
{
    [Fact]
    public void CompletarRegistroGoogle_SinSolicitud_RedirigeAlLogin()
    {
        var controller = CrearController(new UsuarioServicioFalso());

        var resultado = controller.CompletarRegistroGoogle();

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
    }

    [Fact]
    public void CompletarRegistroGoogle_ConSolicitud_MuestraCorreoConfirmado()
    {
        var controller = CrearController(new UsuarioServicioFalso());
        controller.HttpContext.Session.SetString("GoogleRegistroEmail", "cliente@gmail.com");
        controller.HttpContext.Session.SetString("GoogleRegistroNombre", "Cliente Google");

        var resultado = controller.CompletarRegistroGoogle();

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal("cliente@gmail.com", controller.ViewBag.EmailGoogle);
        Assert.Equal("Cliente Google", controller.ViewBag.NombreGoogle);
    }

    [Fact]
    public async Task CompletarRegistroGoogle_SinAceptarTerminos_NoCreaUsuario()
    {
        var servicio = new UsuarioServicioFalso();
        var controller = CrearController(servicio);
        controller.HttpContext.Session.SetString("GoogleRegistroEmail", "cliente@gmail.com");
        controller.HttpContext.Session.SetString("GoogleRegistroNombre", "Cliente Google");

        var resultado = await controller.CompletarRegistroGoogle("Uruguay", false);

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal(0, servicio.RegistrosSolicitados);
        Assert.Equal("Debés aceptar los términos y condiciones.", controller.ViewBag.ErrorGoogle);
    }

    private static LoginController CrearController(IUsuarioServicio servicio)
    {
        var contexto = new DefaultHttpContext();
        contexto.Session = new SesionEnMemoria();
        return new LoginController(servicio)
        {
            ControllerContext = new ControllerContext { HttpContext = contexto }
        };
    }

    private sealed class SesionEnMemoria : ISession
    {
        private readonly Dictionary<string, byte[]> _datos = new();
        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _datos.Keys;
        public void Clear() => _datos.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _datos.Remove(key);
        public void Set(string key, byte[] value) => _datos[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) =>
            _datos.TryGetValue(key, out value);
    }

    private sealed class UsuarioServicioFalso : IUsuarioServicio
    {
        public int RegistrosSolicitados { get; private set; }
        public Usuario? Login(string email, string contrasenia) => null;
        public (bool Exito, string Error, int UsuarioId) RegistrarCliente(string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia) { RegistrosSolicitados++; return (true, "", 1); }
        public (bool Exito, string Error, int UsuarioId) RegistrarShaper(string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia, string nombreDeNegosio, string contacto) => (false, "", 0);
        public (bool Exito, string Error, int UsuarioId) RegistrarAdmin(string email, string nombre, Pais pais, string contrasenia, string confirmarContrasenia) => (false, "", 0);
        public Usuario? BuscarPorId(int id) => null;
        public Usuario? BuscarPorEmail(string email) => null;
        public List<Shaper> ObtenerShapers() => new();
        public Shaper? ObtenerShaperPorId(int id) => null;
        public int ContarClientes() => 0;
        public int ContarClientes(string busqueda) => 0;
        public List<ClienteAdminItem> ObtenerClientesPaginados(string busqueda, int pagina, int cantidadPorPagina) => new();
        public bool CambiarEstadoCliente(int id, bool activo) => false;
        public int ContarShapersActivos() => 0;
        public (bool Exito, string Error) ActualizarShaper(int id, string email, string nombre, Pais pais, string nombreDeNegosio, string contacto) => (false, "");
        public bool ActualizarLogoShaper(int id, string? logoUrl) => false;
        public bool CambiarEstadoShaper(int id, bool activo) => false;
        public (bool Exito, string Error) ActualizarCuenta(int id, string email, string nombre, Pais pais) => (false, "");
        public (bool Exito, string Error) CambiarContrasenia(int id, string contraseniaActual, string nuevaContrasenia, string confirmarContrasenia) => (false, "");
        public List<Shaper> ObtenerShapersPaginados(string busqueda, int pagina, int cantidadPorPagina) => new();
        public int ContarShapers(string busqueda) => 0;
    }
}
