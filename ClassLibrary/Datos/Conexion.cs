using System.Data.SqlClient;

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
            "Server=tcp:zephyrdev.database.windows.net,1433;Initial Catalog=SurfDB;" +
            "Persist Security Info=False;User ID=Zephyr;Password=Surf1234;" +
            "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;" +
            "Connection Timeout=30;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}
