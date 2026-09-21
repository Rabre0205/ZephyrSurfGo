using ClassLibrary.Enums;
using ClassLibrary.Persona;
using System.Data;
using System.Data.SqlClient;

namespace ClassLibrary.Datos
{
    public interface IShaperRepositorio
    {
        Shaper? ObtenerPorId(int id);
    }

    public class ShaperRepositorio : IShaperRepositorio
    {
        public Shaper? ObtenerPorId(int id)
        {
            string sql = @"
                SELECT
                    Id,
                    Email,
                    Contrasenia,
                    Nombre,
                    Pais,
                    NombreDeNegosio,
                    Contacto,
                    LogoUrl
                FROM Usuarios
                WHERE Id = @Id";

            using SqlConnection conexion =
                Conexion.ObtenerConexion();

            using SqlCommand comando =
                new SqlCommand(sql, conexion);

            comando.Parameters.Add(
                "@Id",
                SqlDbType.Int
            ).Value = id;

            conexion.Open();

            using SqlDataReader lector =
                comando.ExecuteReader();

            if (!lector.Read())
            {
                return null;
            }

            return new Shaper(
                id: lector.GetInt32(0),
                email: lector.GetString(1),
                contrasenia: lector.GetString(2),
                nombre: lector.GetString(3),
                pais: (Pais)lector.GetInt32(4),
                nombreDeNegosio: lector.GetString(5),
                contacto: lector.GetString(6),
                logoUrl: lector.IsDBNull(7)
                    ? string.Empty
                    : lector.GetString(7)
            );
        }
    }
}