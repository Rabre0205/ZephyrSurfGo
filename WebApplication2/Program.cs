using ClassLibrary.Datos;
using ClassLibrary.Servicios;
using CloudinaryDotNet;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            DotEnv.Load(options: new DotEnvOptions());

            builder.Services.AddDataProtection();

            builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
            builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
            builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
            builder.Services.AddScoped<IProductoServicio, ProductoServicio>();
            builder.Services.AddScoped<ICloudinaryServicio, CloudinaryServicio>();
            builder.Services.AddScoped<ICifradoServicio, CifradoServicio>();
            builder.Services.AddScoped<ICredencialesMercadoPagoRepositorio, CredencialesMercadoPagoRepositorio>();
            builder.Services.AddScoped<IMercadoPagoServicio, MercadoPagoServicio>();
            builder.Services.AddScoped<ICarritoRepositorio, CarritoRepositorio>();
            builder.Services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
            builder.Services.AddScoped<IPedidoServicio, PedidoServicio>();
            builder.Services.AddScoped<ISolicitudPersonalizadaRepositorio, SolicitudPersonalizadaRepositorio>();
            builder.Services.AddScoped<ISolicitudPersonalizadaServicio, SolicitudPersonalizadaServicio>();
            builder.Services.AddScoped<IPuntoRetiroRepositorio, PuntoRetiroRepositorio>();
            builder.Services.AddScoped<IPuntoRetiroServicio, PuntoRetiroServicio>();
            builder.Services.AddScoped<ISolicitudSoporteRepositorio, SolicitudSoporteRepositorio>();
            builder.Services.AddScoped<ISolicitudSoporteServicio, SolicitudSoporteServicio>();
            builder.Services.AddScoped<IDisenoShaperRepositorio, DisenoShaperRepositorio>();
            builder.Services.AddScoped<IDisenoShaperServicio, DisenoShaperServicio>();
     

            //cosa de cloudinary, echo por claude ni idea que es
            builder.Services.AddSingleton(sp =>
            {
                var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
                var cloudinary = new Cloudinary(cloudinaryUrl);
                cloudinary.Api.Secure = true;
                return cloudinary;
            });

            //cosas de authentication, echo por claude ni idea que es
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Login/Logout";
                options.AccessDeniedPath = "/Login/AccesoDenegado";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            })
            .AddCookie("GoogleTemporal", options =>
            {
                options.Cookie.Name = "Zephyr.Google.Temporal";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddGoogle(options =>
            {
                options.SignInScheme = "GoogleTemporal";
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                    ?? throw new InvalidOperationException("Falta configurar Authentication:Google:ClientId.");
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                    ?? throw new InvalidOperationException("Falta configurar Authentication:Google:ClientSecret.");
                options.Events.OnCreatingTicket = context =>
                {
                    if (context.User.TryGetProperty("verified_email", out var verificado) ||
                        context.User.TryGetProperty("email_verified", out verificado))
                    {
                        bool correoVerificado = verificado.ValueKind switch
                        {
                            System.Text.Json.JsonValueKind.True => true,
                            System.Text.Json.JsonValueKind.String =>
                                bool.TryParse(verificado.GetString(), out bool valor) && valor,
                            _ => false
                        };

                        context.Identity?.AddClaim(
                            new System.Security.Claims.Claim(
                                "google_email_verified",
                                correoVerificado.ToString().ToLowerInvariant()));
                    }

                    return Task.CompletedTask;
                };
            });

            // If you have Razor Pages:
            builder.Services.AddRazorPages();

            // If you also have MVC controllers/views:
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute(
                "/Home/Error",
                "?statusCode={0}");

            app.UseHttpsRedirection();

            // serve static files (wwwroot)
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            //cosa de auth
            app.UseAuthentication();


            app.UseAuthorization();

            // Map Razor Pages first so Pages respond to "/"
            app.MapRazorPages();

            // Map controllers (if any)
            app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Surf}/{action=Home}/{id?}");

            // If MapStaticAssets is required, map it AFTER pages/controllers:
            // app.MapStaticAssets();

            app.Run();
        }
    }
}
