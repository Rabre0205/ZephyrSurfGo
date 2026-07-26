using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Pagos
{
    public class CredencialesMercadoPago
    {
        public int UsuarioId { get; set; }
        public long MercadoPagoUserId { get; set; }
        public string AccessTokenCifrado { get; set; }
        public string RefreshTokenCifrado { get; set; }
        public DateTime TokenExpira { get; set; }
    }
}
