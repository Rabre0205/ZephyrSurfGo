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
        private readonly string _clientId = Environment.GetEnvironmentVariable("MP_CLIENT_ID");
        private readonly string _clientSecret = Environment.GetEnvironmentVariable("MP_CLIENT_SECRET");
        private readonly string _redirectUri = Environment.GetEnvironmentVariable("MP_REDIRECT_URI");
        private readonly decimal _comision = decimal.Parse(Environment.GetEnvironmentVariable("MP_COMISION_PLATAFORMA"));

        public MercadoPagoServicio(ICredencialesMercadoPagoRepositorio credencialesRepositorio, ICifradoServicio cifradoServicio)
        {
            _credencialesRepositorio = credencialesRepositorio;
            _cifradoServicio = cifradoServicio;
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
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _credencialesRepositorio.Guardar(
                shaperId,
                datos.UserId,
                _cifradoServicio.Cifrar(datos.AccessToken),
                _cifradoServicio.Cifrar(datos.RefreshToken),
                DateTime.UtcNow.AddSeconds(datos.ExpiresIn));
        }

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
