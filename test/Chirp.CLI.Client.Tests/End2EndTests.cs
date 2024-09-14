using System.Diagnostics;

public class End2End 
{
     public record DataRecord(string Author, string Message, long Timestamp);

    [Fact]
    public void TestReadCheep()
    {
        Console.WriteLine("test");
        // Arrange
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // var dbPath = Path.Combine(baseDirectory, "../../../../../data/chirp_cli_db.csv");
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "chirp_cli_db.csv");
        var db = new CSVDatabase<DataRecord>(dbPath);

        // Act
        string output = "";
        using (var process = new Process())
        {
            process.StartInfo.FileName = "dotnet";
            //Console.WriteLine("This is DBPATH " + dbPath);
            // The argument
            process.StartInfo.Arguments = "./src/Chirp.CLI.Client/bin/Debug/net7.0/Chirp.CLI.dll read 10";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = "../../../../../";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            // Synchronously read the standard output of the spawned process.
            StreamReader reader = process.StandardOutput;
            output = reader.ReadToEnd();
            process.WaitForExit();
        }

        string fstCheep = output.Split("\n")[1].Trim();

        // Assert
        Console.WriteLine(fstCheep);
        Assert.StartsWith("ropf", fstCheep);
        bool endsWithExpected = fstCheep.EndsWith("@ 01-08-2023 12:09:20 Hello, BDSA students!");
        Assert.True(endsWithExpected, $"Expected the cheep to end with the correct string, but got: {fstCheep}");
    }
}