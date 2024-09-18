using SimpleDB;
using System.Globalization;
using CsvHelper;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
// Our code implementation here:
app.MapGet("/cheeps", (int? limit) => ReadFromDB(limit));

app.MapPost("/cheep", (Cheep cheep) => PostToDB(cheep));

app.Run();

var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");

var database = new CSVDatabase<Cheep>(dbPath);

class DBService<T>
{
    string _filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");

    IEnumerable<T> ReadFromDB(int? limit = null)
    {
        IEnumerable<T> records = new List<T>();
        using (StreamReader reader = new StreamReader(_filepath))
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            records = csv.GetRecords<T>().ToList().Take(limit ?? int.MaxValue);
        }
        return records;
    }

    void PostToDB(T record)
    {
        using (StreamWriter writer = new StreamWriter(_filepath, true))
        using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.NextRecord();
            csv.WriteRecord(record);
        }
    }
}
