namespace Chirp.Razor.test;

public class UnitTests
{
    
    [Theory]
    [InlineData("Helge", "Hello, BDSA students!", 1690892208)]
    //[InlineData("Adrian", "Hej, velkommen til kurset.", 1690895308)]
    public void TestReadForAuthor (string authorData, string messageData, long timestampData) 
    {
        // Arrange
        // Get CHIRPDB Environment Variable
        string chirpDbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");

        // Check if CHIRPDBPATH is set.
        if (string.IsNullOrEmpty(chirpDbPath))
        {
            // string tempDir = Path.GetTempPath();
            chirpDbPath = Path.Combine(Path.GetTempPath(), "mychirp.db");
        }
        
        var dbFacade = new DBFacade(chirpDbPath);
        var cheepService = new CheepService(dbFacade);
        
        // Act
        var cheepList = cheepService.GetCheepsFromAuthor(authorData, 1);
        var cheep = cheepList.First();
     
        
        // Assert
        Assert.Equal(cheep.Author, authorData);
        Assert.Equal(cheep.Message, messageData);
        Assert.Equal(cheep.Timestamp, UnixTimeStampToDateTimeString(timestampData));
    }
    
     [Fact]
     public void TestReadallcheeps() 
     {
         // Arrange
         CheepViewModel cheep = new CheepViewModel(@"Helge", 
                                                  "Hello, BDSA students!", 
                                                 "1690892208");
         // Get CHIRPDB Environment Variable
         string chirpDbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
    
         // Check if CHIRPDBPATH is set.
         if (string.IsNullOrEmpty(chirpDbPath))
         {
             // string tempDir = Path.GetTempPath();
             chirpDbPath = Path.Combine(Path.GetTempPath(), "mychirp.db");
         }
         
         var dbFacade = new DBFacade(chirpDbPath);
         var cheepService = new CheepService(dbFacade);
         
         // Act
         var cheepList = cheepService.GetCheeps(1);
         var First_cheep = cheepList.First();
      
         
         // Assert
         Assert.Equal(First_cheep.Author, cheep.Author);
         Assert.Equal(First_cheep.Message, cheep.Message);
         Assert.Equal(First_cheep.Timestamp, UnixTimeStampToDateTimeString(Double.Parse(cheep.Timestamp)));
     }
    
    
    
    
    
    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("dd/MM/yy H:mm:ss");
    }
}