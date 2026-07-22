using ClassLibrary.Datos;
using ClassLibrary.Servicios;
using CloudinaryDotNet;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //cosas de Cloudinary, echo por claude ni idea que es
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            
            builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
            builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
            builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
            builder.Services.AddScoped<IProductoServicio, ProductoServicio>();
            builder.Services.AddScoped<ICloudinaryServicio, CloudinaryServicio>();

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
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
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