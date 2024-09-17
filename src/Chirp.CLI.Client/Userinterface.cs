using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
public static class Userinterface
{
    public static void PrintCheeps(IEnumerable<Cheep> cheeps, int limit)
    {
        // If limit is not provided set limit to total
        if (limit == 0)
        {
            limit = cheeps.Count();
        }

        // If limit exceeds amount of cheeps, print out all cheeps and inform the user
        if (limit > cheeps.Count())
        {
            Console.WriteLine($"There are less cheeps stored than the requested limit, printing out {cheeps.Count()} cheeps");
            foreach (Cheep cheep in cheeps)
            {
                Console.WriteLine($"{cheep.Author} @ {ConvertFromUnixTimestamp(cheep.Timestamp)} {cheep.Message}");
            }
        }
        else
        {
            // Print out only the amount of cheeps within the limit
            foreach (var cheep in cheeps.Take(limit))
            {
                Console.WriteLine($"{cheep.Author} @ {ConvertFromUnixTimestamp(cheep.Timestamp)} {cheep.Message}");
            }
        }
    }

    public static String ConvertFromUnixTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToString("dd-MM-yyyy HH:mm:ss");
    }
}
