using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using WebApplication2.Models;

namespace WebApplication2.Servicios;

public interface ICorreoNotificacionServicio
{
    Task<bool> EnviarSolicitudShaperAsync(UnirseShaperViewModel solicitud);
}

public class CorreoNotificacionServicio(IConfiguration configuracion, ILogger<CorreoNotificacionServicio> logger)
    : ICorreoNotificacionServicio
{
    public async Task<bool> EnviarSolicitudShaperAsync(UnirseShaperViewModel solicitud)
    {
        string? usuario = configuracion["Correo:SmtpUsuario"];
        string? contrasena = configuracion["Correo:SmtpContrasena"];
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            logger.LogWarning("No se envió la solicitud de shaper porque faltan las credenciales SMTP.");
            return false;
        }

        string destino = configuracion["Correo:DestinoSolicitudesShaper"] ?? "zephyrsurfgo@gmail.com";
        string host = configuracion["Correo:SmtpHost"] ?? "smtp.gmail.com";
        int puerto = configuracion.GetValue("Correo:SmtpPuerto", 587);
        string seguro(string valor) => HtmlEncoder.Default.Encode(valor);

        using var mensaje = new MailMessage
        {
            From = new MailAddress(usuario, "Zephyr Surf Go"),
            Subject = $"Nueva solicitud de shaper: {solicitud.Marca}",
            IsBodyHtml = true,
            Body = $"""
                <h2>Nueva solicitud para unirse como shaper</h2>
                <p><strong>Nombre:</strong> {seguro(solicitud.Nombre)}</p>
                <p><strong>Correo:</strong> {seguro(solicitud.Email)}</p>
                <p><strong>Marca o taller:</strong> {seguro(solicitud.Marca)}</p>
                <p><strong>Ubicación:</strong> {seguro(solicitud.Ubicacion)}</p>
                <p><strong>Celular/WhatsApp:</strong> {seguro(solicitud.Celular)}</p>
                <p><strong>Instagram:</strong> {seguro(solicitud.Instagram)}</p>
                <p><strong>Experiencia:</strong> {solicitud.AniosExperiencia} años</p>
                <p><strong>Tablas completamente personalizadas:</strong> {(solicitud.RealizaPersonalizadas ? "Sí" : "No")}</p>
                <p><strong>Realiza envíos:</strong> {(solicitud.RealizaEnvios ? "Sí" : "No")}</p>
                <p><strong>Presentación:</strong><br>{seguro(solicitud.Presentacion).Replace("\n", "<br>")}</p>
                """
        };
        mensaje.To.Add(destino);
        mensaje.ReplyToList.Add(new MailAddress(solicitud.Email, solicitud.Nombre));

        try
        {
            using var cliente = new SmtpClient(host, puerto)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(usuario, contrasena)
            };
            await cliente.SendMailAsync(mensaje);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo enviar la solicitud de shaper de {Email}.", solicitud.Email);
            return false;
        }
    }
}
