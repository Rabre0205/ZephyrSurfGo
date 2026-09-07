using Microsoft.AspNetCore.Http;

namespace ClassLibrary.Servicios
{
    public interface ICloudinaryServicio
    {
        string SubirImagen(IFormFile archivo, string nombrePublico);
    }
}
