public class DatabaseTests
{
    [Fact]
    public void TestWriteReadCheep()
    {
        // Arrange
        var db = new CSVDatabase<Cheep>("chirp_cli_db.csv");
        string message = "Hello World!";
        string author = "John Doe";
        long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var record = new Cheep(message, author, timeStamp);

        // Act
        //db.Store(cheep);
        db.Store(record);
        Cheep result = db.Read(1).First();
        // Console.log("outCheep: ", outCheep);
        
        // Assert
        // format is: adho @ 08/02/23 14:37:38: I hope you had a good summer.
        Assert.Equal(author, result.Author);
        Assert.Equal(message, result.Message);
        Assert.Equal(timeStamp, result.Timestamp);

    }

    // [Fact]
    // public void TestReadWriteMultipleCheeps() {
    //     // Arrange

    //     // Act
        
    //     // Assert
    // }
}
