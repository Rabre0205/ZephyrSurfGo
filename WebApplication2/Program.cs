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

            app.UseAuthorization();

            // Map Razor Pages first so Pages respond to "/"
            app.MapRazorPages();

            // Map controllers (if any)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // If MapStaticAssets is required, map it AFTER pages/controllers:
            // app.MapStaticAssets();

            app.Run();
        }
    }
}