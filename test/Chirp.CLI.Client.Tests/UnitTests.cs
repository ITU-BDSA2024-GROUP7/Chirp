using System;

namespace UnitTests.cs
{
    /*
    public class PrimeService
    {
        public bool Program.cs(User user)
        {

            throw new NotImplementedException("Not implemented.");
        }
    }
    */

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
        string convertUnixToRealTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
        
        Assert.AreEqual("8/1/2023 10:16:00", convertUnixToRealTime, 0.001, "Convertion succeeds")
    }
    
    // Test 2 Potentially check for if after converting from Unix to real time, if it converts correctly back again

    // Test 3 Testing for if the instructions which comes up when you run "dotnet run" are correct
    
}