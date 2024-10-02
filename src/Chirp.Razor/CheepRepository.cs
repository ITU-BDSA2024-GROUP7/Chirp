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
        // Define the query - with our setup, EF Core translates this to an SQLite query in the background
        var query = from cheep in _dbContext.Cheeps
            where cheep.Author.Name == "Adrian"
            select new { cheep.Text, cheep.Author };
            // Execute the query and store the results
        var result = await query.ToListAsync();
        
        Cheep newCheep = new() { Text = cheep.Text, Author = cheep.AuthorId, TimeStamp = long.Parse(cheep.TimeStamp) };
        
        // Messages Text , Author som skal reference til, System date time to long
        
        var queryResult = await _dbContext.cheep.AddAsync(newMessage); // does not write to the database!

        await _dbContext.SaveChangesAsync(); // persist the changes in the database
        return queryResult.Entity.MessageId;
    }
}

public class CheepDTO
{
    public string Text { get; set; }
    public int AuthorId { get; set; }
    public long TimeStamp { get; set; }
}