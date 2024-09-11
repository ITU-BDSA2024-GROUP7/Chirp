public class DatabaseTests
{
    public record DataRecord(string Author, string Message, long Timestamp);

    [Theory]
    [InlineData("John Doe", "Hello World!", 1645480000)]
    [InlineData("Jane Doe", "Goodbye World!", 1645480001)]
    public void TestWriteReadRecord(string authorData, string messageData, long timestampData)
    {
        // Arrange
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var dbPath = Path.Combine(baseDirectory, "../../../../../data/chirp_cli_db.csv");
        var db = new CSVDatabase<DataRecord>(dbPath);
        var testRecord = new DataRecord(authorData, messageData, timestampData);

        // Act
        db.Store(testRecord);
        var result = db.Read(1).Last();

        // Assert
        Assert.Equal(authorData, result.Author);
        Assert.Equal(messageData, result.Message);
        Assert.Equal(timestampData, result.Timestamp);
    }

    [Fact]
    public void TestReadWriteMultipleRecords()
    {
        // Arrange
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var dbPath = Path.Combine(baseDirectory, "../../../../../data/chirp_cli_db.csv");
        var db = new CSVDatabase<Cheep>(dbPath);
        long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var record1 = new Cheep("John Doe", "Hello World!", timeStamp);
        var record2 = new Cheep("Jane Doe", "Goodbye World!", timeStamp);

        // Act
        db.Store(record1);
        db.Store(record2);
        // Cheep result = db.Read(1).Last();
        List<Cheep> resultList = db.Read().ToList();

        // Get last and second last records
        Cheep result1 = resultList[resultList.Count - 2]; // John Doe
        Cheep result2 = resultList[resultList.Count - 1]; // Jane Doe

        // Assert
        // John Doe
        Assert.Equal("John Doe", result1.Author);
        Assert.Equal("Hello World!", result1.Message);
        Assert.Equal(timeStamp, result1.Timestamp);
        // Jane Doe
        Assert.Equal("Jane Doe", result2.Author);
        Assert.Equal("Goodbye World!", result2.Message);
        Assert.Equal(timeStamp, result2.Timestamp);
    }

    [Fact]
    public void TestReadWriteCheep()
    {
        // Arrange
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var dbPath = Path.Combine(baseDirectory, "../../../../../data/chirp_cli_db.csv");
        var db = new CSVDatabase<Cheep>(dbPath);
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
