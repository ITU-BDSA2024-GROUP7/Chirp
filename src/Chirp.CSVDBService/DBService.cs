// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();

using SimpleDB;

class DBService<T>
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    WebApplication app = builder.Build();
    // Our code implementation here:

    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");
    var database = new CSVDatabase<Cheep>(dbPath);

    public void StartService()
    {
        app.MapGet("/cheeps", (int? limit) => ReadFromDB(limit)); 
        app.MapPost("/cheep", (Cheep cheep) => PostToDB(cheep)); 
        app.Run();
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
            csv.NextRecord();
            csv.WriteRecord(record);
        }
    }
}
