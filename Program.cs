﻿using DocoptNet;
using SimpleDB;

const string usage = @"Chirp CLI version.
Usage:
    chirp read
    chirp cheep
";

var arguments = new Docopt().Apply(usage, args, version: "1.0", exit: true)!;
var database = new CSVDatabase<Cheep>("chirp_cli_db.csv");

if (arguments["read"].IsTrue)
{
    try
    {
        List<Cheep> cheeps = database.Read().ToList();
        Userinterface.PrintCheeps(cheeps);
    }
    catch (Exception e)
    {
        Console.WriteLine("Could not read Cheeps: " + e.Message);
    }
}

if (arguments["cheep"].IsTrue)
{
    string message = (args[1]);
    string author = Environment.MachineName;
    long date = DateTimeOffset.Now.ToUnixTimeSeconds();

    database.Store(new Cheep(author, message, date));
}

public record Cheep(string Author, string Message, long Timestamp);