namespace ClassLibrary.Servicios
{
    public interface ICifradoServicio
    {
        string Cifrar(string textoPlano);
        string Descifrar(string textoCifrado);
    }
}
