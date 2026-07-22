using System.Data.SqlClient;

namespace ClassLibrary.Datos
{
    /// Punto único de acceso a la cadena de conexión y creación de SqlConnection.
    public static class Conexion
    {
        public static SqlConnection ObtenerConexion()
        {
            string cadena = Environment.GetEnvironmentVariable("SURFDB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(cadena))
                throw new Exception("Falta configurar SURFDB_CONNECTION_STRING en las variables de entorno.");

            try
            {
                return new SqlConnection(cadena);
            }
            catch (Exception e)
            {
                throw new Exception("Error al crear la conexión a la base de datos: " + e.Message);
            }
        }
    }
}
