using System.Globalization;
using CsvHelper;

class MockServer<T> : IDisposable
{
    private readonly WebApplication app;
    private readonly HttpClient client;
    private readonly string filepath;

    public MockServer()
    {
        filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<DBService<Cheep>>(); /// singleton
        app = builder.Build();

        // Route for posting a new "cheeps"
        app.MapGet("/cheeps", (int? limit, DBService<Cheep> dbService) => dbService.ReadFromDB(limit));
        // Route for posting a new "cheep"
        app.MapPost("/cheep", (Cheep cheep, DBService<Cheep> dbService) => dbService.PostToDB(cheep));

        // Start the web application
        Task.Run(() => app.RunAsync()); // Run the app in a separate task

        // Initialize HttpClient for testing
        client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }

    // returns the servers client
    public HttpClient Client => client;
    public void Dispose()
    {
        client.Dispose(); // Dispose HttpClient
    }

    public IEnumerable<T> ReadFromDB(int? limit = null)
    {
        IEnumerable<T> records = new List<T>();
        using (StreamReader reader = new StreamReader(filepath))
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            records = csv.GetRecords<T>().ToList().Take(limit ?? int.MaxValue);
        }
        return records;
    }

    public void PostToDB(T record)
    {
        using (StreamWriter writer = new StreamWriter(filepath, append: true))
        using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            Console.WriteLine(record);
            csv.NextRecord();
            csv.WriteRecord(record);
        }
    }
}
