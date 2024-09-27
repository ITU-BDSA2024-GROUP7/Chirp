using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chirp.Razor; // Ensure this is the correct namespace for your project
using SQLitePCL;

namespace Chirp.Razor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Initialize SQLite provider
            Batteries.Init(); // Ensure this method is available and correctly referenced

            // Create the WebApplicationBuilder
            var builder = WebApplication.CreateBuilder(args);

            // Get CHIRPDB Environment Variable
            string chirpDbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");

            // Check if CHIRPDBPATH is set
            if (string.IsNullOrEmpty(chirpDbPath))
            {
                chirpDbPath = Path.Combine(Path.GetTempPath(), "mychirp.db");
            }

            // Add services to the container
            builder.Services.AddRazorPages();
            builder.Services.AddSingleton<ICheepService, CheepService>();

            // Register DBFacade with the specified database path
            builder.Services.AddSingleton(sp => new DBFacade(chirpDbPath));

            // Build the application
            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Map Razor Pages
            app.MapRazorPages();

            // Run the application
            app.Run();
        }
    }
}