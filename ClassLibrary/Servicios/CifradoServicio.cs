using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.DataProtection;


namespace ClassLibrary.Servicios
{
    public interface ICifradoServicio
    {
        string Cifrar(string textoPlano);
        string Descifrar(string textoCifrado);
    }

    public class CifradoServicio : ICifradoServicio
    {
        private readonly IDataProtector _protector;

        public CifradoServicio(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("MercadoPago.Credenciales");
        }

        public string Cifrar(string textoPlano) => _protector.Protect(textoPlano);
        public string Descifrar(string textoCifrado) => _protector.Unprotect(textoCifrado);
    }
}
