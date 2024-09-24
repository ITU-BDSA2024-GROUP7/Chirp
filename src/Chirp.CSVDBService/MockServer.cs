using System.Globalization;
using CsvHelper;

class MockServer<T> : IDisposable
{
    private readonly WebApplication app;
    private readonly HttpClient client;

    public static void StartServer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<DBService<Cheep>>(); /// singleton
        app = builder.Build();

        // Route for posting a new "cheeps"
        app.MapGet("/cheeps", (int? limit, DBService<Cheep> dbService) => dbService.ReadFromDB(limit));
        // Route for posting a new "cheep"
        app.MapPost("/cheep", (Cheep cheep, DBService<Cheep> dbService) => dbService.PostToDB(cheep));
        app.Start();
    }

    // returns the servers client
    public HttpClient Client => client;
    public void Dispose()
    {
        app.Dispose(); // Dispose the WebApplication
        client.Dispose(); // Dispose HttpClient
    }
    string _filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");

    public IEnumerable<T> ReadFromDB(int? limit = null)
    {
        IEnumerable<T> records = new List<T>();
        using (StreamReader reader = new StreamReader(_filepath))
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            records = csv.GetRecords<T>().ToList().Take(limit ?? int.MaxValue);
        }
        return records;
    }

    public void PostToDB(T record)
    {
        using (StreamWriter writer = new StreamWriter(_filepath, true))
        using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            Console.WriteLine(record);
            csv.NextRecord();
            csv.WriteRecord(record);
        }
    }
}
