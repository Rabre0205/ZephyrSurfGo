using Microsoft.Data.SqlClient;

namespace ClassLibrary.Datos
{
    /// <summary>
    /// Punto único de acceso a la cadena de conexión y creación de SqlConnection.
    /// Cambió la cadena acá o cargala desde App.config / appsettings del proyecto
    /// consumidor si preferís no tenerla hardcodeada.
    /// </summary>
    public static class Conexion
    {
        private static readonly string cadenaConexion =
            "Server=localhost\\SQLEXPRESS;Database=Surf;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}
