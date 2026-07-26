using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ClassLibrary.Datos
{
    public interface ICredencialesMercadoPagoRepositorio
    {
        void Guardar(int usuarioId, long mpUserId, string accessTokenCifrado, string refreshTokenCifrado, DateTime expira);
        ClassLibrary.Pagos.CredencialesMercadoPago ObtenerPorUsuarioId(int usuarioId);
    }

    public class CredencialesMercadoPagoRepositorio : ICredencialesMercadoPagoRepositorio
    {
        public void Guardar(int usuarioId, long mpUserId, string accessTokenCifrado, string refreshTokenCifrado, DateTime expira)
        {
            string sql = @"
            MERGE CredencialesMercadoPago AS destino
            USING (SELECT @UsuarioId AS UsuarioId) AS origen
            ON destino.UsuarioId = origen.UsuarioId
            WHEN MATCHED THEN UPDATE SET
                MercadoPagoUserId = @MpUserId, AccessTokenCifrado = @AccessToken,
                RefreshTokenCifrado = @RefreshToken, TokenExpira = @Expira
            WHEN NOT MATCHED THEN INSERT (UsuarioId, MercadoPagoUserId, AccessTokenCifrado, RefreshTokenCifrado, TokenExpira)
                VALUES (@UsuarioId, @MpUserId, @AccessToken, @RefreshToken, @Expira);";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                comando.Parameters.Add("@MpUserId", SqlDbType.BigInt).Value = mpUserId;
                comando.Parameters.Add("@AccessToken", SqlDbType.NVarChar).Value = accessTokenCifrado;
                comando.Parameters.Add("@RefreshToken", SqlDbType.NVarChar).Value = refreshTokenCifrado;
                comando.Parameters.Add("@Expira", SqlDbType.DateTime2).Value = expira;

                conexion.Open();
                comando.ExecuteNonQuery();
            }
        }

        public ClassLibrary.Pagos.CredencialesMercadoPago ObtenerPorUsuarioId(int usuarioId)
        {
            string sql = @"SELECT MercadoPagoUserId, AccessTokenCifrado, RefreshTokenCifrado, TokenExpira
                        FROM CredencialesMercadoPago WHERE UsuarioId = @UsuarioId";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
                conexion.Open();

                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (!lector.Read()) return null;

                    return new ClassLibrary.Pagos.CredencialesMercadoPago
                    {
                        MercadoPagoUserId = lector.GetInt64(0),
                        AccessTokenCifrado = lector.GetString(1),
                        RefreshTokenCifrado = lector.GetString(2),
                        TokenExpira = lector.GetDateTime(3)
                    };
                }
            }
        }
    }
}
