using System;
using System.Diagnostics;
public class End2End
{
    public record DataRecord(string Author, string Message, long Timestamp);

    [Fact]
    public void TestReadCheep()
    {
        // Arrange
        string output = "";
        
        // Act
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

        string fstCheep = output.Split("\n")[0].Trim();

        // Assert
        Console.WriteLine(fstCheep);
        Assert.StartsWith("ropf", fstCheep);
        //                                    ropf @ 08-01-2023 12:09:20 Hello, BDSA students!
        bool endsWithExpected = fstCheep.EndsWith("@ 01-08-2023 12:09:20 Hello, BDSA students!");
        Assert.True(endsWithExpected, $"Expected the cheep to end with the correct string, but got: {fstCheep}");
    }

    [Fact]
    public void TestWriteReadCheep()
    {
        // Arrange
        string output = "";
        TimeZoneInfo cetZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        DateTimeOffset dt;

        // Act
        // Run the cheep command
        using (var cheepProcess = new Process())
        {
            cheepProcess.StartInfo.FileName = "dotnet";
            //log time
            dt = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, cetZone);
            // dt = DateTime.Now; // get the current time
            cheepProcess.StartInfo.Arguments = "./src/Chirp.CLI.Client/bin/Debug/net7.0/Chirp.CLI.dll cheep Wassup World!";
            cheepProcess.StartInfo.UseShellExecute = false;
            cheepProcess.StartInfo.WorkingDirectory = "../../../../../";
            cheepProcess.Start();
            cheepProcess.WaitForExit();
        }

        // Run the read command
        using (var readProcess = new Process())
        {
            readProcess.StartInfo.FileName = "dotnet";
            readProcess.StartInfo.Arguments = "./src/Chirp.CLI.Client/bin/Debug/net7.0/Chirp.CLI.dll read";
            readProcess.StartInfo.UseShellExecute = false;
            readProcess.StartInfo.RedirectStandardOutput = true;
            readProcess.StartInfo.WorkingDirectory = "../../../../../";
            readProcess.Start();

            // Synchronously read the standard output of the spawned process.
            using (StreamReader reader = readProcess.StandardOutput)
            {
                output = reader.ReadToEnd();
            }

            readProcess.WaitForExit();
        }
        // Trimmed split to handle different line terminators
        string lastCheep = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim();

        // Assert
        Assert.StartsWith(System.Environment.MachineName, lastCheep);
        bool endsWithExpected = lastCheep.EndsWith($"Wassup World!");
        Assert.True(endsWithExpected, $"Expected the cheep to end with the correct string, but got: {lastCheep}");
    }
}