using System.Globalization;
using CsvHelper;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DBService<Cheep>>(); /// singleton
var app = builder.Build();
// Our code implementation here:
app.MapGet("/cheeps", (int? limit, DBService<Cheep> dbService) => dbService.ReadFromDB(limit)); 

// Route for posting a new "cheep"
app.MapPost("/cheep", (Cheep cheep, DBService<Cheep> dbService) => dbService.PostToDB(cheep));
app.Run();

class DBService<T>
{
    string _filepath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "site", "wwwroot", "data", "chirp_cli_db.csv");

    public IEnumerable<T> ReadFromDB(int? limit = null)
    {
        if (!File.Exists(_filepath))
{
        Console.WriteLine($"File not found: {_filepath}");
        return Enumerable.Empty<T>();
}
        try{
        IEnumerable<T> records = new List<T>();
        using (StreamReader reader = new StreamReader(_filepath))
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            records = csv.GetRecords<T>().ToList().Take(limit ?? int.MaxValue);
        }
        return records;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading from DB " + e);
            return null;
        }
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
