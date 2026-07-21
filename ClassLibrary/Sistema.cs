using ClassLibrary.Datos;
using ClassLibrary.Persona;
using ClassLibrary.Productos;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassLibrary
{
    public class Sistema
    {

        private static Sistema _instancia;

        private readonly UsuarioRepositorio usuarioRepositorio;
        private readonly ProductoRepositorio productoRepositorio;
        private readonly Cloudinary cloudinary;

        public List<Usuario> Usuarios { get; private set; }
        public List<Producto> Productos { get; private set; }
        public Cloudinary Cloudinary { get { return cloudinary; } }

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
            // Cargar variables de entorno desde archivo .env
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

            // Configurar Cloudinary
            var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");

            cloudinary = new Cloudinary(cloudinaryUrl);
            cloudinary.Api.Secure = true;

            usuarioRepositorio = new UsuarioRepositorio();
            productoRepositorio = new ProductoRepositorio();

            Usuarios = new List<Usuario>();
            Productos = new List<Producto>();
            CargarDatos();
        }



        public void CargarDatos()
        {
            Usuarios = usuarioRepositorio.ObtenerTodos();
            Productos = productoRepositorio.ObtenerTodos();
        }

  
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
        public Usuario Login(string email, string contrasenia)
        {
            foreach (Usuario usuario in Usuarios)
            {
                if (usuario.Email == email && usuario.Contrasenia == contrasenia)
                {
                    return usuario;
                }
            }

            return null;
        }

        /// <summary>
        /// Cierra la sesión del usuario logueado.
        /// </summary>


        /// <summary>
        /// Sube una imagen a Cloudinary directamente desde un IFormFile (formulario web).
        /// </summary>
        /// <param name="archivo">Archivo IFormFile del formulario</param>
        /// <param name="nombrePublico">Nombre con el que guardar en Cloudinary (sin extensión)</param>
        /// <returns>URL pública de la imagen subida, o null si falla</returns>
        public string SubirImagenFormularioACloudinary(Microsoft.AspNetCore.Http.IFormFile archivo, string nombrePublico)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                {
                    throw new ArgumentException("El archivo está vacío o no es válido.");
                }

                // Crear parámetros de carga desde stream
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(archivo.FileName, archivo.OpenReadStream()),
                    PublicId = nombrePublico,
                    Overwrite = true  // Sobrescribir si ya existe
                };

                // Subir a Cloudinary
                var uploadResult = cloudinary.Upload(uploadParams);

                // Retornar URL segura (HTTPS)
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return uploadResult.SecureUrl.ToString();
                }
                else
                {
                    throw new Exception($"Error al subir imagen: {uploadResult.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en SubirImagenFormularioACloudinary: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Agrega una nueva Tabla a la base de datos con imágenes cargadas desde un formulario web.
        /// Las imágenes se suben directamente a Cloudinary sin pasar por almacenamiento local.
        /// </summary>
        /// <param name="titulo">Título de la tabla</param>
        /// <param name="subtitulo">Subtítulo</param>
        /// <param name="precio">Precio</param>
        /// <param name="descripcion">Descripción detallada</param>
        /// <param name="shaperId">ID del Shaper propietario</param>
        /// <param name="altura">Altura de la tabla (ej: "6'2\"")</param>
        /// <param name="ancho">Ancho en pulgadas</param>
        /// <param name="volumen">Volumen en litros</param>
        /// <param name="sistemaDeEncaje">Enum SistemaDeEncaje</param>
        /// <param name="tipoDeOla">Enum TipoDeOla</param>
        /// <param name="estiloDeSurf">Enum EstiloDeSurf</param>
        /// <param name="pesoMinimo">Peso mínimo recomendado</param>
        /// <param name="pesoMaximo">Peso máximo recomendado</param>
        /// <param name="experiencia">Enum Experiencia requerida</param>
        /// <param name="imagenFrontal">Archivo IFormFile de imagen frontal</param>
        /// <param name="imagenTrasera">Archivo IFormFile de imagen trasera (opcional)</param>
        /// <returns>Id de la tabla insertada, o -1 si falla</returns>
        public int AgregarTabla(
            string titulo, string subtitulo, double precio, string descripcion, int shaperId,
            string altura, int ancho, double volumen,
            ClassLibrary.Enums.SistemaDeEncaje sistemaDeEncaje,
            ClassLibrary.Enums.TipoDeOla tipoDeOla,
            ClassLibrary.Enums.EstiloDeSurf estiloDeSurf,
            int pesoMinimo, int pesoMaximo,
            ClassLibrary.Enums.Experiencia experiencia,
            Microsoft.AspNetCore.Http.IFormFile imagenFrontal,
            Microsoft.AspNetCore.Http.IFormFile imagenTrasera = null)
        {
            try
            {
                // Validar que hay imagen frontal
                if (imagenFrontal == null || imagenFrontal.Length == 0)
                {
                    throw new ArgumentException("La imagen frontal es requerida.");
                }

                // Generar nombres únicos para Cloudinary basados en título y timestamp
                string timestamp = DateTime.Now.Ticks.ToString();
                string nombreImagenFrontal = $"tabla_{titulo.Replace(" ", "_").Replace("'", "")}_{timestamp}_frontal";
                string nombreImagenTrasera = $"tabla_{titulo.Replace(" ", "_").Replace("'", "")}_{timestamp}_trasera";

                // Subir imagen frontal a Cloudinary
                string urlFrontal = SubirImagenFormularioACloudinary(imagenFrontal, nombreImagenFrontal);
                if (string.IsNullOrEmpty(urlFrontal))
                {
                    throw new Exception("No se pudo subir la imagen frontal a Cloudinary.");
                }

                // Subir imagen trasera (opcional)
                string urlTrasera = null;
                if (imagenTrasera != null && imagenTrasera.Length > 0)
                {
                    urlTrasera = SubirImagenFormularioACloudinary(imagenTrasera, nombreImagenTrasera);
                    if (string.IsNullOrEmpty(urlTrasera))
                    {
                        System.Diagnostics.Debug.WriteLine("Advertencia: No se pudo subir la imagen trasera, continuando sin ella.");
                    }
                }

                // Crear objeto Tabla con las URLs de Cloudinary
                var tabla = new Tabla(
                    titulo: titulo,
                    subtitulo: subtitulo,
                    precio: precio,
                    descripcion: descripcion,
                    imagenUrl: urlFrontal,
                    shaperId: shaperId,
                    altura: altura,
                    ancho: ancho,
                    volumen: volumen,
                    sis: sistemaDeEncaje,
                    tipodeola: tipoDeOla,
                    estilo: estiloDeSurf,
                    pesomin: pesoMinimo,
                    pesomax: pesoMaximo,
                    exp: experiencia,
                    urlimg: urlTrasera ?? ""
                );

                // Insertar tabla en la base de datos
                int idGenerado = productoRepositorio.InsertarTabla(tabla);

                // Recargar datos para mantener sincronizada la caché
                CargarDatos();

                return idGenerado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AgregarTabla: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Obtiene todas las Tablas (productos) de un Shaper específico.
        /// </summary>
        /// <param name="shaperId">ID del Shaper</param>
        /// <returns>Lista de Tablas del Shaper, o lista vacía si no hay</returns>
        public List<Tabla> ObtenerTablasDelShaper(int shaperId)
        {
            List<Tabla> tablas = new List<Tabla>();

            foreach (Producto producto in Productos)
            {
                // Verificar que es una Tabla y pertenece al Shaper
                if (producto is Tabla tabla && producto.ShaperId == shaperId)
                {
                    tablas.Add(tabla);
                }
            }

            return tablas;
        }

    }
}
