using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chirp.Razor;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the WebApplicationBuilder
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddRazorPages(); 
            builder.Services.AddScoped<CheepRepository>();
           // builder.Services.AddScoped<ICheepRepository, CheepRepository>();
            

            // Load database connection via configuration
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<CheepDBContext>(options => options.UseSqlite(connectionString));

            // Build the application
            var app = builder.Build();

            // Seed the database after the application is built
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var migrationContext = scope.ServiceProvider.GetService<CheepDBContext>();
                try
                {
                    migrationContext.Database.Migrate(); // Apply pending migrations
                    logger.LogInformation("Migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"An error occurred while applying migrations: {ex.Message}");
                }
                
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<CheepDBContext>();
                
                // Call the SeedDatabase method
                DbInitializer.SeedDatabase(context);
            }

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