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

            // Add GitHub Services
            // builder.Services.AddAuthentication()
            //     .AddGitHub(options =>
            //     {
            //         options.ClientId = builder.Configuration["AUTHENTICATION_GITHUB_CLIENTID"]!;
            //         options.ClientSecret = builder.Configuration["AUTHENTICATION_GITHUB_CLIENTSECRET"]!;
            //         options.CallbackPath = new PathString("/signin-github");
            //     });
            
            builder.Services
                .AddAuthentication(options =>
                {
                    options.RequireAuthenticatedSignIn = true;
                })
                .AddGitHub(options =>
                {
                    options.ClientId = builder.Configuration["AUTHENTICATION_GITHUB_CLIENTID"]!;
                    options.ClientSecret = builder.Configuration["AUTHENTICATION_GITHUB_CLIENTSECRET"]!;
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