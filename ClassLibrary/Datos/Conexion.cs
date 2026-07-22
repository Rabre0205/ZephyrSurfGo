using System.Data.SqlClient;

namespace ClassLibrary.Datos
{
    public static class Conexion
    {
        private static readonly string cadenaConexion =
            "Server=tcp:zephyrdev.database.windows.net,1433;" +
            "Initial Catalog=SurfDB;" +
            "Persist Security Info=False;" +
            "User ID=Zephyr;" +
            "Password=Surf1234;" +
            "MultipleActiveResultSets=False;" +
            "Encrypt=True;" +
            "TrustServerCertificate=False;" +
            "ConnectRetryCount=3;" +
            "ConnectRetryInterval=10;" +
            "Connection Timeout=60;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}