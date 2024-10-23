using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the WebApplicationBuilder
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddRazorPages();

            // Load database connection via configuration
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add the DbContext first
            builder.Services.AddDbContext<CheepDBContext>(options => options.UseSqlite(connectionString)); 

            // Then add Identity services
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<CheepDBContext>();

            // Register your repositories and services
            builder.Services.AddScoped<CheepRepository>();
            builder.Services.AddScoped<CheepService>();

            // Build the application
            var app = builder.Build();

            // Seed the database after the application is built
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<CheepDBContext>();
                
                context.Database.Migrate();
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
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Razor Pages
            app.MapRazorPages();
            
            app.MapGet("/cheeps", async (CheepService cheepService) =>
            {
                var cheeps = await cheepService.RetrieveAllCheeps();
                return Results.Ok(cheeps);
            });
            
            // Run the application
            app.Run();
        }
    }
}