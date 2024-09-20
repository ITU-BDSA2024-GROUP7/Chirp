using System;
using System.Reflection;
using DocoptNet;
using SimpleDB;
using static Userinterface;

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

        new Cheep("Author", "Message", 1690891760);
        // Testing
        //var us = new Userinterface();
        var result = Userinterface.ConvertFromUnixTimestamp(1690891760);
        Assert.Equal("01-08-2023 12:09:20", result);
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

    [Fact]
    public void SingletonCheckIfInstancesAreSame()
    {
        // Arrange
        string filePath = "../../data/chirp_cli_db.csv";

        // Act
        var instance1 = CSVDatabase<Cheep>.Instance(filePath);
        var instance2 = CSVDatabase<Cheep>.Instance(filePath);

        // Assert
        Assert.Same(instance1, instance2);
    }
}