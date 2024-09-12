using System.ComponentModel.Design;
public static class Userinterface {
    public static void PrintCheeps(IEnumerable<Cheep> cheeps) {
        foreach(Cheep cheep in cheeps) {
            Console.WriteLine(cheep.Author + " @ " + ConvertFromUnixTimestamp(cheep.Timestamp) + " " + cheep.Message);
        }    
    }

    public static DateTime ConvertFromUnixTimestamp(long timestamp) {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }
}
