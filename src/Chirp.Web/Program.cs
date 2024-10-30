using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
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

            // Retrieve ClientId and ClientSecret from configuration
            string? clientId = builder.Configuration["AUTHENTICATION_GITHUB_CLIENTID"];
            string? clientSecret = builder.Configuration["AUTHENTICATION_GITHUB_CLIENTSECRET"];

            if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(clientSecret))
            {
                throw new ApplicationException("Failed to retrieve both the Github Client ID and Secret. Make sure that the values are set on the machine.");
            }
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ApplicationException("Failed to retrieve the Github Client ID. Make sure that the github value is set on the machine.");
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ApplicationException("Failed to retrieve the Github Secret. Make sure that the github value is set on the machine.");
            }
            
            // Add GitHub Services
            builder.Services.AddAuthentication()
                .AddGitHub(options =>
                {
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.CallbackPath = new PathString("/signin-github");
                });
            
            builder.Services.AddSession();
            
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
            app.UseSession();
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