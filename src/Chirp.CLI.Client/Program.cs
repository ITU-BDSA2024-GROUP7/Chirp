using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;  // For simplified JSON handling
using System.Text.Json;
using DocoptNet;
using SimpleDB;

const string usage = @"Chirp CLI version.
Usage:
    chirp read [<limit>]
    chirp cheep <message>...
";


var arguments = new Docopt().Apply(usage, args, version: "1.0", exit: true)!;

const string baseUrl = "http://localhost:5118";
using HttpClient client = new();
client.BaseAddress = new Uri(baseUrl);

if (arguments["read"].IsTrue)
{       
    // "/cheeps" --- object of Cheep
    try
    {
        if (arguments.ContainsKey("<limit>") && arguments["<limit>"] != null && !string.IsNullOrEmpty(arguments["<limit>"].ToString()))
        {
            // Called with a limit
            int limit = int.Parse(arguments["<limit>"]!.ToString());
            var response = await client.GetAsync("/cheeps");
            List<Cheep> cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
            Userinterface.PrintCheeps(cheeps);
        }
        else
        {
            // Called without any limit
            Console.WriteLine("Reading all Cheeps");
            var response = await client.GetAsync("/cheeps");
            List<Cheep> cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
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

    await client.PostAsJsonAsync("/cheep", new Cheep(author, message, date));
    
   // database.Store(new Cheep(author, message, date));
}

public record Cheep(string Author, string Message, long Timestamp);
// this is for creating a new cheep record