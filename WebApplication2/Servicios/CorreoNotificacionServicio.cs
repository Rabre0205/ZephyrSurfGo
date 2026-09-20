using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using WebApplication2.Models;
using ClassLibrary.Soporte;

namespace WebApplication2.Servicios;

public interface ICorreoNotificacionServicio
{
    Task<bool> EnviarSolicitudShaperAsync(UnirseShaperViewModel solicitud);
    Task<bool> EnviarConsultaSoporteAsync(SolicitudSoporte solicitud);
    Task<bool> EnviarAUsuarioAsync(int usuarioId, string asunto, string titulo, string mensaje);
    Task<bool> EnviarAEmailAsync(string email, string asunto, string titulo, string mensaje);
}

public class CorreoNotificacionServicio(IConfiguration configuracion, ILogger<CorreoNotificacionServicio> logger,
    ClassLibrary.Datos.IUsuarioRepositorio usuarios)
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

    public async Task<bool> EnviarConsultaSoporteAsync(SolicitudSoporte solicitud)
    {
        string? usuario = configuracion["Correo:SmtpUsuario"];
        string? contrasena = configuracion["Correo:SmtpContrasena"];
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            logger.LogWarning("No se notificó la consulta #{Id} porque faltan las credenciales SMTP.", solicitud.Id);
            return false;
        }

        string destino = configuracion["Correo:DestinoSoporte"]
            ?? configuracion["Correo:DestinoSolicitudesShaper"]
            ?? "zephyrsurfgo@gmail.com";
        string host = configuracion["Correo:SmtpHost"] ?? "smtp.gmail.com";
        int puerto = configuracion.GetValue("Correo:SmtpPuerto", 587);
        string seguro(string valor) => HtmlEncoder.Default.Encode(valor ?? string.Empty);

        using var mensaje = new MailMessage
        {
            From = new MailAddress(usuario, "Zephyr Surf Go"),
            Subject = $"Consulta de soporte #{solicitud.Id}: {solicitud.Asunto}",
            IsBodyHtml = true,
            Body = $"""
                <h2>Nueva consulta desde el Centro de ayuda</h2>
                <p><strong>Número:</strong> #{solicitud.Id}</p>
                <p><strong>Shaper:</strong> {seguro(solicitud.ShaperNombre)}</p>
                <p><strong>Correo:</strong> {seguro(solicitud.ShaperEmail)}</p>
                <p><strong>Asunto:</strong> {seguro(solicitud.Asunto)}</p>
                <p><strong>Mensaje:</strong><br>{seguro(solicitud.Mensaje).Replace("\n", "<br>")}</p>
                <p>La consulta también quedó registrada en el panel administrativo.</p>
                """
        };
        mensaje.To.Add(destino);
        if (MailAddress.TryCreate(solicitud.ShaperEmail, out var respuestaA))
            mensaje.ReplyToList.Add(respuestaA);

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
            logger.LogError(ex, "No se pudo notificar por correo la consulta de soporte #{Id}.", solicitud.Id);
            return false;
        }
    }

    public Task<bool> EnviarAUsuarioAsync(int usuarioId, string asunto, string titulo, string mensaje)
    {
        var usuario = usuarios.ObtenerPorId(usuarioId);
        return usuario == null || !usuario.Activo
            ? Task.FromResult(false)
            : EnviarAEmailAsync(usuario.Email, asunto, titulo, mensaje);
    }

    public async Task<bool> EnviarAEmailAsync(string email, string asunto, string titulo, string mensaje)
    {
        string? usuario = configuracion["Correo:SmtpUsuario"];
        string? contrasena = configuracion["Correo:SmtpContrasena"];
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena) ||
            !MailAddress.TryCreate(email, out var destino))
            return false;

        string host = configuracion["Correo:SmtpHost"] ?? "smtp.gmail.com";
        int puerto = configuracion.GetValue("Correo:SmtpPuerto", 587);
        string seguro(string valor) => HtmlEncoder.Default.Encode(valor ?? string.Empty);
        using var correo = new MailMessage
        {
            From = new MailAddress(usuario, "Zephyr Surf Go"),
            Subject = asunto,
            IsBodyHtml = true,
            Body = $"<div style=\"font-family:Arial,sans-serif;max-width:620px\"><h2>{seguro(titulo)}</h2><p style=\"line-height:1.6\">{seguro(mensaje).Replace("\n", "<br>")}</p><p style=\"color:#6b6255;font-size:13px\">Podés consultar el detalle iniciando sesión en Zephyr Surf Go.</p></div>"
        };
        correo.To.Add(destino);
        try
        {
            using var cliente = new SmtpClient(host, puerto) { EnableSsl = true, Credentials = new NetworkCredential(usuario, contrasena) };
            await cliente.SendMailAsync(correo);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo enviar la notificación {Asunto}.", asunto);
            return false;
        }
    }
}
