using System;
using System.Reflection;
using DocoptNet;


public class UnitTests
{
    // Used to simulate DocoptNet library inside the test enviorenment
    private const string usage = @"Chirp CLI version.
    Usage:
        chirp read
        chirp cheep

    Options:
        -h --help     Show this screen.
        --version     Show version.
    ";

    
    // Testing for if Unix Timestamp is converted correctly
    [Fact]
    private void TestUnixTimeStampConversion(){

        // Setting up test cheep
        string author = "Author";
        string message = "Message";
        long unixTimestamp = 1690891760; // Just a test timestamp

        // Put the cheep together
        var cheep = new Cheep(author, message, unixTimestamp);

        // Converting Unix to Real Time
        string convertUnixToRealTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.ToString("M/d/yyyy HH:mm:ss");
        
        Assert.Equal("8-1-2023 12:09:20", convertUnixToRealTime);
    }
    
    
    [Fact]
    public void TestingReadCommand()
    {
        var args = new[] {"read"};
        var docopt = new Docopt();
        var arguments = docopt.Apply(usage, args, version: "Chirp CLI 1.0", help: true);
        Assert.True(arguments["read"].IsTrue, "read command should parse as true");
    }

    [Fact]
    public void TestingCheepCommand(){
        var args = new[] {"cheep"};
        var docopt = new Docopt();
        var arguments = docopt.Apply(usage, args, version: "Chirp CLI 1.0", help: true);
        Assert.True(arguments["cheep"].IsTrue, "cheep command should parse as true");
    }
}