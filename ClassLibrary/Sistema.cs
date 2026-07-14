using ClassLibrary.Persona;
using ClassLibrary.Productos;
using ClassLibrary.Datos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary
{
    public class Sistema
    {
        private static Sistema _instancia;

        private readonly UsuarioRepositorio usuarioRepositorio;
        private readonly ProductoRepositorio productoRepositorio;

        public List<Usuario> Usuarios { get; private set; }
        public List<Producto> Productos { get; private set; }
        public Usuario UsuarioLogueado { get; set; }

        /// <summary>
        /// Singleton: obtiene o crea la instancia única del sistema.
        /// </summary>
        public static Sistema ObtenerInstancia()
        {
            if (_instancia == null)
            {
                _instancia = new Sistema();
            }
            return _instancia;
        }

        private Sistema()
        {
            usuarioRepositorio = new UsuarioRepositorio();
            productoRepositorio = new ProductoRepositorio();

            Usuarios = new List<Usuario>();
            Productos = new List<Producto>();
            UsuarioLogueado = null;

            CargarDatos();
        }

        /// <summary>
        /// Recarga todos los datos desde la base. Llamalo después de cualquier
        /// operación de escritura (alta, baja, modificación) para mantener la
        /// caché en memoria sincronizada con la DB.
        /// </summary>
        public void CargarDatos()
        {
            Usuarios = usuarioRepositorio.ObtenerTodos();
            Productos = productoRepositorio.ObtenerTodos();
        }

        /// <summary>
        /// Búsqueda de usuario por Id sin LINQ: recorrido manual con foreach.
        /// </summary>
        public Usuario BuscarUsuarioPorId(int id)
        {
            foreach (Usuario usuario in Usuarios)
            {
                if (usuario.Id == id)
                {
                    return usuario;
                }
            }

            return null;
        }

        /// <summary>
        /// Búsqueda de usuario por Email sin LINQ.
        /// </summary>
        public Usuario BuscarUsuarioPorEmail(string email)
        {
            foreach (Usuario usuario in Usuarios)
            {
                if (usuario.Email == email)
                {
                    return usuario;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtiene los productos de un Shaper específico sin LINQ.
        /// </summary>
        public List<Producto> BuscarProductosPorShaper(int shaperId)
        {
            List<Producto> resultado = new List<Producto>();

            foreach (Producto producto in Productos)
            {
                if (producto.ShaperId == shaperId)
                {
                    resultado.Add(producto);
                }
            }

            return resultado;
        }

        /// <summary>
        /// Autentica un usuario buscando por Email y Contraseña.
        /// Retorna un Usuario o Shaper si las credenciales son correctas, null si no existe.
        /// También establece UsuarioLogueado si el login es exitoso.
        /// </summary>
        public Usuario Login(string email, string contrasenia)
        {
            // Búsqueda sin LINQ: recorrido manual del diccionario en memoria
            foreach (Usuario usuario in Usuarios)
            {
                if (usuario.Email == email && usuario.Contrasenia == contrasenia)
                {
                    // Login exitoso: establecer usuario logueado
                    UsuarioLogueado = usuario;
                    return usuario;
                }
            }

            // No encontrado o contraseña incorrecta
            return null;
        }

        /// <summary>
        /// Cierra la sesión del usuario logueado.
        /// </summary>
        public void Logout()
        {
            UsuarioLogueado = null;
        }
    }
}
