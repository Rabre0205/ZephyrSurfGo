using ClassLibrary.Datos;
using ClassLibrary.Enums;
using ClassLibrary.Persona;
using ClassLibrary.Servicios;

namespace Pruebas;

public class UsuarioServicioTests
{
    [Fact]
    public void LoginAceptaCredencialesCorrectasDeUsuarioActivo()
    {
        const string clave = "ClaveSegura123";
        var usuario = CrearUsuario(clave, activo: true);
        var servicio = new UsuarioServicio(new UsuarioRepositorioFalso(usuario));

        var resultado = servicio.Login("CLIENTE@EJEMPLO.COM", clave);

        Assert.Same(usuario, resultado);
    }

    [Fact]
    public void LoginRechazaContraseniaIncorrecta()
    {
        var usuario = CrearUsuario("ClaveSegura123", activo: true);
        var servicio = new UsuarioServicio(new UsuarioRepositorioFalso(usuario));

        Assert.Null(servicio.Login(usuario.Email, "ClaveIncorrecta"));
    }

    [Fact]
    public void LoginRechazaUsuarioBloqueado()
    {
        const string clave = "ClaveSegura123";
        var usuario = CrearUsuario(clave, activo: false);
        var servicio = new UsuarioServicio(new UsuarioRepositorioFalso(usuario));

        Assert.Null(servicio.Login(usuario.Email, clave));
    }

    [Fact]
    public void ActualizarCuentaRechazaCorreoDeOtroUsuario()
    {
        var actual = CrearUsuario("ClaveSegura123", activo: true);
        var otro = new Usuario(
            9, "otro@ejemplo.com", "Otro", Pais.Uruguay,
            BCrypt.Net.BCrypt.HashPassword("OtraClave123"));
        var repositorio = new UsuarioRepositorioFalso(actual, otro);
        var servicio = new UsuarioServicio(repositorio);

        var resultado = servicio.ActualizarCuenta(
            actual.Id, otro.Email, actual.Nombre, actual.Pais);

        Assert.False(resultado.Exito);
        Assert.Contains("Ya existe", resultado.Error);
        Assert.False(repositorio.SeActualizoCuenta);
    }

    [Fact]
    public void CambiarContraseniaRechazaLaClaveActualIncorrecta()
    {
        var usuario = CrearUsuario("ClaveSegura123", activo: true);
        var repositorio = new UsuarioRepositorioFalso(usuario);
        var servicio = new UsuarioServicio(repositorio);

        var resultado = servicio.CambiarContrasenia(
            usuario.Id, "Incorrecta", "NuevaClave123", "NuevaClave123");

        Assert.False(resultado.Exito);
        Assert.Contains("actual es incorrecta", resultado.Error);
        Assert.False(repositorio.SeActualizoContrasenia);
    }

    private static Usuario CrearUsuario(string clave, bool activo) => new(
        4,
        "cliente@ejemplo.com",
        "Cliente",
        Pais.Uruguay,
        BCrypt.Net.BCrypt.HashPassword(clave))
    {
        Activo = activo,
        TipoDeUsuario = TipoDeUsuario.Cliente
    };

    private sealed class UsuarioRepositorioFalso : IUsuarioRepositorio
    {
        private readonly List<Usuario> _usuarios;

        public bool SeActualizoCuenta { get; private set; }
        public bool SeActualizoContrasenia { get; private set; }

        public UsuarioRepositorioFalso(params Usuario[] usuarios) =>
            _usuarios = usuarios.ToList();

        public Usuario? ObtenerPorId(int id) =>
            _usuarios.SingleOrDefault(usuario => usuario.Id == id);

        public Usuario? ObtenerPorEmail(string email) =>
            _usuarios.SingleOrDefault(usuario => string.Equals(
                usuario.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

        public bool ActualizarCuenta(int id, string email, string nombre, Pais pais)
        {
            SeActualizoCuenta = true;
            return true;
        }

        public bool ActualizarContrasenia(int id, string contraseniaHash)
        {
            SeActualizoContrasenia = true;
            return true;
        }
        public bool ActualizarLogoShaper(int id, string? logoUrl) => true;

        public List<Usuario> ObtenerTodos() => _usuarios.ToList();
        public List<Shaper> ObtenerShapersPaginados(string busqueda, int pagina, int cantidadPorPagina) => new();
        public int ContarShapers(string busqueda) => 0;
        public int ContarUsuariosPorTipo(TipoDeUsuario tipo) => _usuarios.Count(usuario => usuario.TipoDeUsuario == tipo);
        public int ContarClientes(string busqueda) => 0;
        public List<ClienteAdminItem> ObtenerClientesPaginados(string busqueda, int pagina, int cantidadPorPagina) => new();
        public bool CambiarEstadoCliente(int id, bool activo) => false;
        public int ContarShapersActivos() => 0;
        public int InsertarUsuario(Usuario usuario) => 0;
        public int InsertarShaper(Shaper shaper) => 0;
        public bool CambiarEstadoShaper(int id, bool activo) => false;
        public bool ActualizarShaper(int id, string email, string nombre, Pais pais, string nombreDeNegosio, string contacto) => false;
    }
}
