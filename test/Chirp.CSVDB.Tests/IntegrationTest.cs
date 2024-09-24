public class DatabaseTests
{
    public record DataRecord(string Author, string Message, long Timestamp);

    [Theory]
    [InlineData("John Doe", "Hello World!", 1645480000)]
    [InlineData("Jane Doe", "Goodbye World!", 1645480001)]
    public async Task TestWriteReadRecord(string authorData, string messageData, long timestampData)
    {
        // Arrange
        using var server = new MockServer<Cheep>();
        var client = server.Client;
        var testRecord = new DataRecord(authorData, messageData, timestampData);

        // Act
        var postResponse = await client.PostAsJsonAsync("/cheep", testRecord);
        postResponse.EnsureSuccessStatusCode();

        // Fetch all cheeps to verify it was stored
        var response = await client.GetAsync("/cheeps");
        response.EnsureSuccessStatusCode();
        List<Cheep> cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
        var result = cheeps.Last();

        // Assert
        Assert.Equal(authorData, result.Author);
        Assert.Equal(messageData, result.Message);
        Assert.Equal(timestampData, result.Timestamp);
    }

    [Fact]
    public async Task TestReadWriteMultipleRecords()
    {
        // Arrange
        using var server = new MockServer<Cheep>(); // Using ensure that Dispose() is called at the end of the block
        var client = server.Client;

        long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var record1 = new Cheep("John Doe", "Hello World!", timeStamp);
        var record2 = new Cheep("Jane Doe", "Goodbye World!", timeStamp);

        // Act
        var postResponse = await client.PostAsJsonAsync("/cheep", record1);
        postResponse.EnsureSuccessStatusCode();
        postResponse = await client.PostAsJsonAsync("/cheep", record2);
        postResponse.EnsureSuccessStatusCode();

        // Fetch all cheeps to verify it was stored
        var response = await client.GetAsync("/cheeps");
        response.EnsureSuccessStatusCode();
        List<Cheep> cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();

        // Get last and second last records
        Cheep result1 = cheeps[cheeps.Count - 2]; // John Doe
        Cheep result2 = cheeps[cheeps.Count - 1]; // Jane Doe

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
    public async Task TestReadWriteCheep()
    {
        // Arrange
        using var server = new MockServer<Cheep>();
        var client = server.Client;

        string author = "MelonMusk";
        string message = "Wait this is not X the everything app";
        long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var cheep = new Cheep(author, message, timeStamp);

        // Act
        var postResponse = await client.PostAsJsonAsync("/cheep", cheep);
        postResponse.EnsureSuccessStatusCode();

        // Fetch all cheeps to verify it was stored
        var response = await client.GetAsync("/cheeps");
        response.EnsureSuccessStatusCode();
        List<Cheep> cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
        var result = cheeps.Last();

        // Assert
        Assert.Equal(author, result.Author);
        Assert.Equal(message, result.Message);
        Assert.Equal(timeStamp, result.Timestamp);
    }
}
