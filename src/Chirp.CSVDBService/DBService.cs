// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();

using SimpleDB;

class DBService
{
    
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    WebApplication app = builder.Build();
    // Our code implementation here:

    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");
    var database = new CSVDatabase<Cheep>(dbPath);

    public void StartService() {
        app.MapGet("/cheeps", () => database.Read());
        app.MapPost("/cheep", (Cheep cheep) => database.Cheep());
        app.Run();
    }
        
    public void ReadFromDB() {

    }

    public void PostToDB(Cheep cheep) {

    }
}
