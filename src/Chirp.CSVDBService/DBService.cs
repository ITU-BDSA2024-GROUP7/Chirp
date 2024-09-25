using System.Globalization;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Linq;


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
    string _filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");
    public DBService()
    {
        // Ensure that the directory exists
        string directory = Path.GetDirectoryName(_filepath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Ensure that the file exists
        if (!_filepath.Exists(_filepath))
        {
            // Create empty csv file at filepath
            using (var writer = new StreamWriter(_filepath, false));
        }
    }

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
