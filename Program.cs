
using CsvHelper;if (args[0] == "read")
{
    try
    {
        using (StreamReader sr = new StreamReader("chirp_cli_db.csv")) // dispose of the StreamReader after use so no use to close the reader
        {
            string line;
            sr.ReadLine();
            while (sr.ReadLine() != null)
            {
                line = sr.ReadLine();
                string[] status = line.Split('"');
                status[0] = status[0].Trim(',');
                status[2] = status[2].Trim(',');
                Console.WriteLine(status[0] + " @ " + UnixConversion(int.Parse(status[2])).DateTime + " " + status[1]);
            }
        }
        
    }
    catch (Exception e){
        Console.WriteLine("Error");
    }
}

if (args[0] == "cheep")
{
    string message = args[1];
    string author = Environment.MachineName;
    string date = DateTime.Now.ToString("yyyyMMdd");
    
    string csv = author + ",\"" + message + "\"," + date;
    StreamWriter sw = new StreamWriter("chirp_cli_db.csv", true); // consistency using in streamReader but not in StreamWriter
    sw.WriteLine(csv);
    sw.Close();
    
}

DateTimeOffset UnixConversion(int unixTime) {
    return DateTimeOffset.FromUnixTimeSeconds(unixTime);
}

public record Cheep(string Author, string Message, long Timestamp);