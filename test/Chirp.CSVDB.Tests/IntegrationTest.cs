public class DatabaseTests
{
    public record Info(string author, string message, long timeStamp);

    [Theory]
    [InlineData("John Doe", "Hello World!", 1645480000)]
    [InlineData("Jane Doe", "Goodbye World!", 1645480001)]
    public void TestWriteReadRecord(string author, string message, long timeStamp)
    {
        // Arrange
        var db = new CSVDatabase<Cheep>("C:/Users/nik/OneDrive/Documents/GitHub/Chirp/data/chirp_cli_db.csv");
        var testRecord = new Info(author, message, timeStamp);
        // record testRecord = new record(author, message, timeStamp);

        // Act
        db.Store(testRecord);
        Cheep result = db.Read(1).Last();

        // Assert
        Assert.Equal(author, result.Author);
        Assert.Equal(message, result.Message);
        Assert.Equal(timeStamp, result.Timestamp);
    }

    [Fact]
    public void TestReadWriteMultipleRecords()
    {
        // Arrange
        var db = new CSVDatabase<Cheep>("C:/Users/nik/OneDrive/Documents/GitHub/Chirp/data/chirp_cli_db.csv");
        string message = "Hello World!";
        string author = "John Doe";
        long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var record = new Cheep(author, message, timeStamp);

        // Act
        //db.Store(cheep);
        db.Store(record);
        Cheep result = db.Read(1).Last();
        // Console.log("outCheep: ", outCheep);

        // Assert
        // format is: adho @ 08/02/23 14:37:38: I hope you had a good summer.
        Assert.Equal(author, result.Author);
        Assert.Equal(message, result.Message);
        Assert.Equal(timeStamp, result.Timestamp);
    }

    [Fact]
    public void TestReadWriteCheep()
    {
        // Arrange
        var db = new CSVDatabase<Cheep>("C:/Users/nik/OneDrive/Documents/GitHub/Chirp/data/chirp_cli_db.csv");
        string author = "MelonMusk";
        string message = "Wait this is not X the everuthing app";
        long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var cheep = new Cheep(author, message, timeStamp);

        // Act
        db.Store(cheep);
        Cheep result = db.Read(1).Last();

        // Assert
        Assert.Equal(author, result.Author);
        Assert.Equal(message, result.Message);
        Assert.Equal(timeStamp, result.Timestamp);
    }
}
