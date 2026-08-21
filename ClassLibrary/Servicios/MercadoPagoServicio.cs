using ClassLibrary.Datos;
using MercadoPago.Client;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using ClassLibrary.Pedidos;
using ClassLibrary.Pagos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Globalization;
namespace ClassLibrary.Servicios
{
    public interface IMercadoPagoServicio
    {
        string ObtenerUrlAutorizacion(int shaperId);
        Task ProcesarCallbackAsync(string code, int shaperId);
        Task<string> CrearPreferenciaAsync(Pedido pedido);
    }

    public class MercadoPagoServicio : IMercadoPagoServicio
    {
        private readonly ICredencialesMercadoPagoRepositorio _credencialesRepositorio;
        private readonly ICifradoServicio _cifradoServicio;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly decimal _comision;

        public MercadoPagoServicio(ICredencialesMercadoPagoRepositorio credencialesRepositorio, ICifradoServicio cifradoServicio)
        {
            _credencialesRepositorio = credencialesRepositorio;
            _cifradoServicio = cifradoServicio;
            _clientId = ObtenerVariableRequerida("MP_CLIENT_ID");
            _clientSecret = ObtenerVariableRequerida("MP_CLIENT_SECRET");
            _redirectUri = ObtenerVariableRequerida("MP_REDIRECT_URI");

            if (!decimal.TryParse(
                    ObtenerVariableRequerida("MP_COMISION_PLATAFORMA"),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out _comision))
            {
                throw new InvalidOperationException(
                    "MP_COMISION_PLATAFORMA debe contener un número válido.");
            }
        }

        public string ObtenerUrlAutorizacion(int shaperId)
        {
            return "https://auth.mercadopago.com/authorization"
                + $"?client_id={_clientId}&response_type=code&platform_id=mp"
                + $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}&state={shaperId}";
        }

        public async Task ProcesarCallbackAsync(string code, int shaperId)
        {
            using var http = new HttpClient();
            var body = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", _redirectUri }
        };

            var respuesta = await http.PostAsync("https://api.mercadopago.com/oauth/token",
                new FormUrlEncodedContent(body));
            respuesta.EnsureSuccessStatusCode();

            var datos = JsonSerializer.Deserialize<MercadoPagoTokenResponse>(
                await respuesta.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException(
                    "Mercado Pago devolvió una respuesta vacía o inválida.");

            _credencialesRepositorio.Guardar(
                shaperId,
                datos.UserId,
                _cifradoServicio.Cifrar(datos.AccessToken),
                _cifradoServicio.Cifrar(datos.RefreshToken),
                DateTime.UtcNow.AddSeconds(datos.ExpiresIn));
        }

        private static string ObtenerVariableRequerida(string nombre) =>
            Environment.GetEnvironmentVariable(nombre) is { Length: > 0 } valor
                ? valor
                : throw new InvalidOperationException(
                    $"Falta configurar la variable de entorno {nombre}.");

        public async Task<string> CrearPreferenciaAsync(Pedido pedido)
        {
            var credenciales = _credencialesRepositorio.ObtenerPorUsuarioId(pedido.ShaperId);
            if (credenciales == null)
                throw new InvalidOperationException("El shaper todavía no conectó su cuenta de MercadoPago.");

            string accessToken = _cifradoServicio.Descifrar(credenciales.AccessTokenCifrado);

            var itemsPreferencia = new List<PreferenceItemRequest>();
            foreach (PedidoItem item in pedido.Items)
            {
                itemsPreferencia.Add(new PreferenceItemRequest
                {
                    Title = item.TituloSnapshot,
                    Quantity = item.Cantidad,
                    CurrencyId = "UYU",
                    UnitPrice = (decimal)item.PrecioUnitarioSnapshot
                });
            }

            var request = new PreferenceRequest
            {
                Items = itemsPreferencia,
                MarketplaceFee = (decimal)pedido.ComisionPlataforma,
                ExternalReference = pedido.Id.ToString(),
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://tudominio.com/Pedido/PagoExitoso",
                    Failure = "https://tudominio.com/Pedido/PagoFallido",
                    Pending = "https://tudominio.com/Pedido/PagoPendiente"
                }
            };

            // Ojo acá: NO uses MercadoPagoConfig.AccessToken (es estático/global).
            // Con varios shapers cobrando en paralelo, dos requests concurrentes
            // pisarían el token del otro. Usá RequestOptions por request.
            var opciones = new RequestOptions { AccessToken = accessToken };

            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request, opciones);

            return preference.InitPoint;
        }
    }
}
