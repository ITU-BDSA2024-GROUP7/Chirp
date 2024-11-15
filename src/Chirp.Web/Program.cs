using System.Security.Claims;
using Chirp.Infrastructure.Data;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Chirp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the WebApplicationBuilder
            var builder = WebApplication.CreateBuilder(args);
            
            //CORS 
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy  =>
                    {
                        policy.WithOrigins("https://bdsagroup07chirprazor.azurewebsites.net/",
                            "http://localhost:");
                    });
            });
            
            // Add services to the container
            builder.Services.AddRazorPages();
            
            // Once you are sure everything works, you might want to increase this value to up to 1 or 2 years
            builder.Services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(700));
            
            // Load database connection via configuration
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add the DbContext first
            builder.Services.AddDbContext<CheepDBContext>(options => options.UseSqlite(connectionString));
            
            // Then add Identity services
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount = true)
                .AddSignInManager<SignInManager<ApplicationUser>>()
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
                    options.Scope.Add("user:email");
                    options.Scope.Add("user:email");

                    options.Events.OnCreatingTicket = context =>
                    {
                        // Retrieve user details from claims
                        var userName = context.Identity?.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
                        var email = context.Identity?.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;

                        // You can use these values as needed in your application
                        Console.WriteLine($"GitHub Username: {userName}");
                        Console.WriteLine($"GitHub Email: {email}");

                        return Task.CompletedTask;
                    };
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
            
            // // set the Content-Security-Policy header
            // app.Use(async (context, next) =>
            // {
            //     // The Content-Security-Policy header helps to protect the webapp from XSS attacks
            //     context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self';");
            //     await next();
            // });
            
            app.Use(async (context, next) =>
            {
                // The Content-Security-Policy header helps to protect the webapp from XSS attacks.
                // Added connect-src to allow WebSocket connections
                context.Response.Headers.Append("Content-Security-Policy", 
                    "default-src 'self'; " +                            // Allow resources from the same origin
                    "script-src 'self' https://bdsagroup07chirprazor.azurewebsites.net/; " +  // Allow scripts from self and Azure
                    "style-src 'self' 'unsafe-inline'; " +               // Allow inline styles and styles from self
                    "img-src 'self'; " +                                 // Allow images from self
                    "script-src-elem 'self' 'unsafe-inline'; " +         // Allow inline scripts in elements
                    "connect-src 'self' ws://localhost:53540/ wss://localhost:53539/ https://bdsagroup07chirprazor.azurewebsites.net/; " + // Allow WebSocket connections from localhost and Azure
                    "font-src 'self'; " +                                // Allow fonts from self
                    "frame-src 'self'; " +                               // Allow frames from self
                    "object-src 'none'; " +                              // Disallow object elements
                    "worker-src 'self';");                               // Allow workers from self
                await next();
            });

            
            

            //Use CORS
            app.UseCors();
            
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