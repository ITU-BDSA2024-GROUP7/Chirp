using DocoptNet;
using SimpleDB;

const string usage = @"Chirp CLI version.
Usage:
    chirp read [<limit>]
    chirp cheep <message>...
";


var arguments = new Docopt().Apply(usage, args, version: "1.0", exit: true)!;
var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");
var database = new CSVDatabase<Cheep>(dbPath);



if (arguments["read"].IsTrue)
{
    try
    {
        if (arguments.ContainsKey("<limit>") && arguments["<limit>"] != null && !string.IsNullOrEmpty(arguments["<limit>"].ToString()))
        {
            int limit = int.Parse(arguments["<limit>"]!.ToString());
            List<Cheep> cheeps = database.Read(limit).ToList();
            Userinterface.PrintCheeps(cheeps);
        }
        else
        {
            Console.WriteLine("Reading all Cheeps");
            List<Cheep> cheeps = database.Read().ToList();
            Userinterface.PrintCheeps(cheeps);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine("Could not read Cheeps: " + e.Message);
    }
}

if (arguments["cheep"].IsTrue)
{
    string message = string.Join(" ", args.Skip(1));
    string author = Environment.MachineName;
    // Conversion to correct time zone
    TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
    DateTimeOffset cetTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, cetZone);
    long date = cetTime.ToUnixTimeSeconds();

    database.Store(new Cheep(author, message, date));
}

public record Cheep(string Author, string Message, long Timestamp);
// this is for creating a new cheep record