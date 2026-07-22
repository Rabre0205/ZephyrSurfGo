using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary.Servicios
{
    public interface ICloudinaryServicio
    {
        string SubirImagen(IFormFile archivo, string nombrePublico);
    }

    public class CloudinaryServicio : ICloudinaryServicio
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryServicio(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }


        public string SubirImagen(IFormFile archivo, string nombrePublico)
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("El archivo está vacío o no es válido.");

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(archivo.FileName, archivo.OpenReadStream()),
                PublicId = nombrePublico,
                Overwrite = true
            };

            var uploadResult = _cloudinary.Upload(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                return uploadResult.SecureUrl.ToString();

            throw new Exception($"Error al subir imagen: {uploadResult.Error?.Message}");
        }
    }
}
