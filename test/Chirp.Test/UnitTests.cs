using Chirp.Core;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Test;

public class UnitTests
{
    [Theory]
    [InlineData("Helge", "Hello, BDSA students!", 1690892208)]
    //[InlineData("Adrian", "Hej, velkommen til kurset.", 1690895308)]
    public async void TestReadForAuthor (string authorName, string messageData, long unixTimestamp) 
    {
        // Arrange
        using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();                              
        var builder = new DbContextOptionsBuilder<CheepDBContext>().UseSqlite(connection);
                                                           
        using var context = new CheepDBContext(builder.Options);   
        await context.Database.EnsureCreatedAsync();               
        
        // Seed the database with a test entry
        var author = new Author() { AuthorId = 1, Cheeps = null, Email = "mymail", Name = authorName };
        
        var cheep = new Cheep
        {
            CheepId = 1,
            Author = author,
            AuthorId = author.AuthorId,
            Text = messageData,
            TimeStamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime // Ensure this matches the format in your model
        };

        context.Authors.Add(author);
        context.Cheeps.Add(cheep);
        await context.SaveChangesAsync();  // Save the seed data to the in-memory database

        
        ICheepRepository repository = new CheepRepository(context);
        
        // Act
        var cheepList = await repository.ReadCheepsFromAuthor(authorName, 1);
        var firstCheep = cheepList.First();
     
        
        // Assert
        Assert.Equal(firstCheep.AuthorName, authorName);
        Assert.Equal(firstCheep.Text, messageData);
        Assert.Equal(firstCheep.FormattedTimeStamp, UnixTimeStampToDateTimeString(unixTimestamp));
    }
    
     [Fact]
     public async void TestReadallcheeps()
     {
         // Arrange
         using var connection = new SqliteConnection("Filename=:memory:");
         await connection.OpenAsync();                              
         var builder = new DbContextOptionsBuilder<CheepDBContext>().UseSqlite(connection);
                                                           
         using var context = new CheepDBContext(builder.Options);   
         await context.Database.EnsureCreatedAsync();               
        
         // Seed the database with a test entry
         var author1 = new Author() { AuthorId = 1, Cheeps = null, Email = "helge@hotmail", Name = "Helge" };
         var author2 = new Author() { AuthorId = 2, Cheeps = null, Email = "", Name = "Adrian" };
        
         var cheep1 = new Cheep
         {
             CheepId = 1,
             Author = author1,
             AuthorId = author1.AuthorId,
             Text = "Sådan!",
             TimeStamp = DateTimeOffset.FromUnixTimeSeconds(1690892208).UtcDateTime // Ensure this matches the format in your model
         };
         var cheep2 = new Cheep
         {
             CheepId = 2,
             Author = author2,
             AuthorId = author2.AuthorId,
             Text = "Dependency Injections are very important.",
             TimeStamp = DateTimeOffset.FromUnixTimeSeconds(1690992208).UtcDateTime // Ensure this matches the format in your model
         };
         var cheep3 = new Cheep
         {
             CheepId = 3,
             Author = author1,
             AuthorId = author1.AuthorId,
             Text = "I like my cold brewed tee very much.",
             TimeStamp = DateTimeOffset.FromUnixTimeSeconds(1691992208).UtcDateTime // Ensure this matches the format in your model
         };
         var cheep4 = new Cheep
         {
             CheepId = 4,
             Author = author2,
             AuthorId = author2.AuthorId,
             Text = "EF Core is goated!!.",
             TimeStamp = DateTimeOffset.FromUnixTimeSeconds(1692992208).UtcDateTime // Ensure this matches the format in your model
         };
         
         context.Authors.Add(author1);
         context.Authors.Add(author2);
         context.Cheeps.Add(cheep1);
         context.Cheeps.Add(cheep2);
         context.Cheeps.Add(cheep3);
         context.Cheeps.Add(cheep4);
         await context.SaveChangesAsync();  // Save the seed data to the in-memory database

        
         ICheepRepository repository = new CheepRepository(context);
        
         // Act
         var cheepList = await repository.ReadAllCheeps(0);
         
     
        
         // Assert
         Assert.True(cheepList.Count() == 4);
     }
    
    
    
    
    
    private static String UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("yyyy-MM-dd H:mm:ss");
    }
}