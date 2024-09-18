using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
public static class Userinterface
{
    public static void PrintCheeps(IEnumerable<Cheep> cheeps)
    {
        // Prints out all cheeps
        foreach (var cheep in cheeps)
        {
            Console.WriteLine($"{cheep.Author} @ {ConvertFromUnixTimestamp(cheep.Timestamp)} {cheep.Message}");
        }
    }

    public static String ConvertFromUnixTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToString("dd-MM-yyyy HH:mm:ss");
    }
} 
