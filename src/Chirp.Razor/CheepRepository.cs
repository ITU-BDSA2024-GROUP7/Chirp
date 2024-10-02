using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public interface ICheepRepository
{
    Task CreateMessage(CheepDTO newCheep);
    Task<List<CheepDTO>> ReadMessages(string userName);
    Task UpdateMessage(CheepDTO alteredMessage);
}

public class CheepRepository : ICheepRepository
{
    private readonly CheepDBContext _dbContext; // dependency injection
    public CheepRepository(CheepDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<CheepDTO>> ReadMessages(string userName)
    {
        // Formulate the query - will be translated to SQL by EF Core
        var query = _dbContext.Cheeps.Select(message => new { message.Author, message.Text });
        // Execute the query
        var result = await query.ToListAsync();
        return result;
        // ...
    }
    
    public async Task CreateMessage(CheepDTO cheep)
    {
        Cheep newMessage = new() { Text = cheep.Text, Author = cheep.Author, TimeStamp = cheep.Timestamp };
        var queryResult = await _dbContext.cheep.AddAsync(newMessage); // does not write to the database!

        await _dbContext.SaveChangesAsync(); // persist the changes in the database
        return queryResult.Entity.MessageId;
    }
}

public class CheepDTO
{
    public String Text { get; set; }
    public String Author { get; set; }
    public long TimeStamp { get; set; }
}