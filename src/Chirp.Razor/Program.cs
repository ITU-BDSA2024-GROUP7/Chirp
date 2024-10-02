using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chirp.Razor;
using Microsoft.EntityFrameworkCore; // Ensure this is the correct namespace for your project
using SQLitePCL;

namespace Chirp.Razor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the WebApplicationBuilder
            var builder = WebApplication.CreateBuilder(args);

            // Get CHIRPDB Environment Variable
            //string chirpDbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");

            // Check if CHIRPDBPATH is set
            // if (string.IsNullOrEmpty(chirpDbPath))
            // {
            //     chirpDbPath = Path.Combine(Path.GetTempPath(), "mychirp.db");
            // }

            // Add services to the container
            builder.Services.AddRazorPages();
            //builder.Services.AddSingleton<ICheepService, CheepService>();
            builder.Services.AddScoped<ICheepRepository, CheepRepository>();

            
            // Load database connection via configuration
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<CheepDBContext>(options => options.UseSqlite(connectionString));
            
            // Register DBFacade with the specified database path
            // builder.Services.AddSingleton(sp => new DBFacade(chirpDbPath));

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