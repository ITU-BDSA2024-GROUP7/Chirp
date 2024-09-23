using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;  // For simplified JSON handling
using System.Text.Json;
using DocoptNet;

const string usage = @"Chirp CLI version.
Usage:
    chirp read [<limit>]
    chirp cheep <message>...
";


var arguments = new Docopt().Apply(usage, args, version: "1.0", exit: true)!;
const string baseUrl = "https://bdsagroup07chirpremotedb-g0a7c9aphdbrbhgn.northeurope-01.azurewebsites.net/";
using HttpClient client = new();
client.BaseAddress = new Uri(baseUrl);

if (arguments["read"].IsTrue)
{       
    await ReadCheeps(arguments, client);
    
}

if (arguments["cheep"].IsTrue)
{
    await PostCheep(arguments, client, args);
}


async Task ReadCheeps(IDictionary<string, ValueObject> arguments, HttpClient client){
    try
    {
        if (arguments.ContainsKey("<limit>") && arguments["<limit>"] != null && !string.IsNullOrEmpty(arguments["<limit>"].ToString()))
        {
            // Called with a limit
            int limit = int.Parse(arguments["<limit>"]!.ToString());
            var response = await client.GetAsync($"/cheeps?limit={limit}");
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

async Task PostCheep(IDictionary<string, ValueObject> arguments, HttpClient client, string[] args){
    try{
        string message = string.Join(" ", args.Skip(1));
        string author = Environment.MachineName;
        // Conversion to correct time zone
        TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        DateTimeOffset cetTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, cetZone);
        long date = cetTime.ToUnixTimeSeconds();

        await client.PostAsJsonAsync("/cheep", new Cheep(author, message, date));
        
    // database.Store(new Cheep(author, message, date));
    }
    catch (Exception e)
    {
        Console.WriteLine("Could not post the wanted Cheep " + e.Message);
    }
    
}

public record Cheep(string Author, string Message, long Timestamp);
// this is for creating a new cheep record
