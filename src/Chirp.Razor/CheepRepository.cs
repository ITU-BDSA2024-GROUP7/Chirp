using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Chirp.Razor
{
    public interface ICheepRepository
    {
        Task CreateMessage(CheepDTO newCheep);
        Task<List<CheepDTO>> ReadMessages(string userName);
        Task UpdateMessage(CheepDTO alteredMessage);
    }

    public class CheepRepository : ICheepRepository
    {
        private readonly CheepDBContext _dbContext;

        public CheepRepository(CheepDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Read messages by a specific user and map to CheepDTO
        public async Task<List<CheepDTO>> ReadMessages(string userName)
        {
            var query = from cheep in _dbContext.Cheeps
                        where cheep.Author.Name == userName
                        select new CheepDTO
                        {
                            AuthorName = cheep.Author.Name,
                            Text = cheep.Text,
                            FormattedTimeStamp = cheep.TimeStamp.ToString() // You might want to format this better
                        };

            // Execute the query and return the list of messages
            return await query.ToListAsync();
        }

        // Create a new message
        public async Task CreateMessage(CheepDTO cheepDTO)
        {
            // Find the author by name
            var author = await (from a in _dbContext.Authors
                                where a.Name == cheepDTO.AuthorName
                                select a).FirstOrDefaultAsync();

            if (author == null)
            {
                throw new Exception($"Author {cheepDTO.AuthorName} not found");
            }

            // Create a new Cheep
            Cheep newCheep = new Cheep
            {
                Text = cheepDTO.Text,
                Author = author,
                TimeStamp = DateTimeOffset.UtcNow.UtcDateTime // Use current timestamp in UNIX format
            };

            // Add the new Cheep to the DbContext
            await _dbContext.Cheeps.AddAsync(newCheep);
            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }

        // Update message (to be implemented)
        public Task UpdateMessage(CheepDTO alteredMessage)
        {
            throw new NotImplementedException();
        }
    }

    // Data Transfer Object for Cheep
    public class CheepDTO
    {
        public string AuthorName { get; set; } // Author's name
        public string Text { get; set; } // Message text
        public string FormattedTimeStamp { get; set; } // Time stamp as a formatted string
    }

    // Data Transfer Object for Author
    public class AuthorDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
