
using System.Globalization;
using CsvHelper;if (args[0] == "read")
{
    try
    {
        List<Cheep> cheeps = new List<Cheep>();
        using (StreamReader sr = new StreamReader("chirp_cli_db.csv"))
        using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<Cheep>();
            foreach (var record in records)
            {
                cheeps.Add(record);
                //Console.WriteLine(record.Author + " @ " + UnixConversion(record.Timestamp).DateTime + " " + record.Message);
            }
            Userinterface.PrintCheeps(cheeps);
        }
    }
    catch (Exception e){
        Console.WriteLine("Error: " + e.Message);
    }
    
}

if (args[0] == "cheep")
{
    string message = (args[1]);
    string author = Environment.MachineName;
    long date = DateTimeOffset.Now.ToUnixTimeSeconds();
    
    using (StreamWriter sw = new StreamWriter("chirp_cli_db.csv",true))
    using (var csvWriter = new CsvWriter(sw, CultureInfo.InvariantCulture))
    {
        csvWriter.NextRecord();
        csvWriter.WriteRecord(new Cheep(author, message, date));
        
    }
        
    
    
}

DateTimeOffset UnixConversion(long unixTime) {
    return DateTimeOffset.FromUnixTimeSeconds(unixTime);
}

public record Cheep(string Author, string Message, long Timestamp);