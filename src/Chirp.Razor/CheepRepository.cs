using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Chirp.Razor
{
    public interface ICheepRepository
    {
        Task<List<CheepDTO>> ReadCheepsFromAuthor(string userName, int page);
        Task<List<CheepDTO>> ReadAllCheeps(int page);
        Task CreateCheep(CheepDTO newCheep);
        Task UpdateCheep(CheepDTO alteredCheep);
    }
    
    public class CheepRepository : ICheepRepository
    {
        private readonly CheepDBContext _dbContext;

        public CheepRepository(CheepDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Read messages by a specific user and map to CheepDTO
        public async Task<List<CheepDTO>> ReadCheepsFromAuthor(string userName, int page)
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

        public async Task<List<CheepDTO>> ReadAllCheeps(int page)
        {
            var query = _dbContext.Cheeps
                .Include(c => c.Author) 
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip(page * 32)
                .Take(32)
                .Select(cheep => new CheepDTO
                {
                    AuthorName = cheep.Author.Name,
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString()
                });
            // Execute the query and return the list of messages
            return await query.ToListAsync();
        }

        // Create a new message
        public async Task CreateCheep(CheepDTO cheepDTO)
        {
            // Find the author by name
            var author = FindAuthorByName(cheepDTO.AuthorName);
            
            if (author == null)
            {
                await CreateAuthor();
                
                author = FindAuthorByName(cheepDTO.AuthorName);
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
        public Task UpdateCheep(CheepDTO alteredCheep)
        {
            throw new NotImplementedException();
        }
        // Find The author by name
        public Author? FindAuthorByName(String name)
        {
            var author = (from a in _dbContext.Authors
                where a.Name == name
                select a).FirstOrDefault();
            return author;
        }
        
        // Find a user by their email
        public Author? FindAuthorByEmail(string email)
        {
            var author = (from a in _dbContext.Authors
                where a.Email == email
                select a).FirstOrDefault();
            return author;
        }
        
        // Used for creating a new author when the author is not existing
        public async Task CreateAuthor()
        {
            var author = new Author()
            {
                Name = Environment.UserName,
                Email = Environment.UserName + "@example.com",
                Cheeps = new List<Cheep>(),
            };
            await _dbContext.Authors.AddAsync(author);
            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }
        
    }
}
