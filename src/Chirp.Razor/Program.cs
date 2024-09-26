using Chirp.Razor;
using SQLitePCL;

// Initialize SQLite provider
Batteries.Init(); // Ensure this method is available and correctly referenced

var builder = WebApplication.CreateBuilder(args);

// Get CHIRPDB Environment Variable
string chirpDbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");

// Check if CHIRPDBPATH is set.
if (string.IsNullOrEmpty(chirpDbPath))
{
    // string tempDir = Path.GetTempPath();
    chirpDbPath = Path.Combine(Path.GetTempPath(), "mychirp.db");
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ICheepService, CheepService>();

// Register DBFacade with the speicified database path
builder.Services.AddSingleton(sp => new DBFacade(chirpDbPath));

var app = builder.Build();  

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
