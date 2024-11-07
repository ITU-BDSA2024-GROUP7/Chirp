using Chirp.Core;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Infrastructure.Repositories
{
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
            var query = _dbContext.Cheeps
                .Include(c => c.Author) 
                .Where(cheep => cheep.Author.Name == userName)
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((page - 1) * 32)
                .Take(32)
                .Select(cheep => new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString()
                });

            return await query.ToListAsync();
        }

        public async Task<List<CheepDTO>> ReadAllCheeps(int page)
        {
            var query = _dbContext.Cheeps
                .Include(c => c.Author) 
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((page - 1) * 32)
                .Take(32)
                .Select(cheep => new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")
                });

            return await query.ToListAsync();
        }
        // get total count of pages 
        public async Task<int> GetTotalPages(string authorName = null)
        {
            var query = _dbContext.Cheeps.AsQueryable();
            
            // Apply the where clause if there is a valid authorName.
            if (!string.IsNullOrEmpty(authorName))
            {
                query = query.Where(cheep => cheep.Author.Name == authorName);
            }

            var totalCheeps = await query.CountAsync();

            return (int)Math.Ceiling((double)totalCheeps / 32); // Math.Ceiling (round up) to ensure all pages
        }
        // Create a new message
        public async Task CreateCheep(CheepDTO cheepDTO)
        {
            // Find the author by name
            var author = FindAuthorByName(cheepDTO.Author.Name);
            
            if (author == null)
            {
                await CreateAuthor(cheepDTO.Author.Name);
                author = FindAuthorByName(cheepDTO.Author.Name);
            }

            // Create a new Cheep 
            Cheep newCheep = new Cheep
            {
                Text = cheepDTO.Text,
                Author =  author,
                TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
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
        public async Task CreateAuthor(string authorName)
        {
            var author = new Author()
            {   
                Name = authorName,
                Email = authorName,
                Cheeps = new List<Cheep>(),
            };
            await _dbContext.Authors.AddAsync(author);
            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }
        
        public async Task DeleteCheep(int cheepId)
        {
            var cheep = await _dbContext.Cheeps.FindAsync(cheepId);
            if (cheep != null)
            {
                _dbContext.Cheeps.Remove(cheep);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        public async Task DeleteCheepByAuthor(AuthorDTO Author)
        {
            var cheeps = await _dbContext.Cheeps
                .Where(cheep => cheep.Author.Name == Author.Name)
                .ToListAsync();
            if (cheeps != null)
            {
                _dbContext.Cheeps.RemoveRange(cheeps);

                // Retrieve the author by name and then remove them
                var author = await _dbContext.Authors
                    .FirstOrDefaultAsync(a => a.Name == Author.Name);

                if (author != null)
                {
                    _dbContext.Authors.Remove(author);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
        
        public async Task<List<CheepDTO>> RetrieveAllCheepsForEndPoint()
        {
            var query = _dbContext.Cheeps
                .Include(c => c.Author) 
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Select(cheep => new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString()
                });
            // Execute the query and return the list of messages
            return await query.ToListAsync();
        }
    }
}
