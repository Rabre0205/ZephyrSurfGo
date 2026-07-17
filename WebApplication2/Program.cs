namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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