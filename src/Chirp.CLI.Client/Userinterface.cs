using System.ComponentModel.Design;
public static class Userinterface {
    public static void PrintCheeps(IEnumerable<Cheep> cheeps) {
        foreach(Cheep cheep in cheeps) {
            Console.WriteLine(cheep.Author + " @ " + DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).DateTime + " " + cheep.Message);
        }    
    }
}
