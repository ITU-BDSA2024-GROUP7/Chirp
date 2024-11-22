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
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")
                });

            return await query.ToListAsync();
        }

        public async Task<List<CheepDTO>> RetrieveAllCheepsFromAnAuthor(string Username)
        {
            var query = _dbContext.Cheeps
                .Include(c => c.Author) 
                .Where(cheep => cheep.Author.Name == Username)
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Select(cheep => new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")
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
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")
                });

            return await query.ToListAsync();
        }

        public async Task<List<CheepDTO>> ReadPrivateCheeps(int page, string userName)
        {
            
            var query = _dbContext.Cheeps
                .Include(c => c.Author) 
                .Where(cheep => FindAuthorByName(userName).AuthorsFollowed.Contains(cheep.Author.Name) || cheep.Author.Name == userName)
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((page - 1) * 32)
                .Take(32)
                .Select(cheep => new CheepDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")
                });

            return await query.ToListAsync();
        }
        
        // get total count of pages 
        public async Task<int> GetTotalPages(string authorName = "")
        {
            var query = _dbContext.Cheeps.AsQueryable();
            
            // Apply the where clause if there is a valid authorName.
            if (!string.IsNullOrEmpty(authorName))
            {
                // Get the author from the database (assuming the 'authors' table contains the relevant information).
                var author = await _dbContext.Authors
                    .Where(a => a.Name == authorName)
                    .FirstOrDefaultAsync();

                if (author != null)
                {
                    // Get all cheeps from the author and the authors they follow
                    query = query
                        .Where(cheep => cheep.Author.Name == authorName 
                                        || author.AuthorsFollowed.Contains(cheep.Author.Name));
                }
            }

            var totalCheeps = await query.CountAsync();

            return (int)Math.Ceiling((double)totalCheeps / 32); // Math.Ceiling (round up) to ensure all pages
        }
        // Create a new message
        public async Task CreateCheep(CheepDTO cheepDTO)
        {
            // Find the author by name
            var author = FindAuthorByName(cheepDTO.Author.Name);
            
            // Create a new Cheep 
            if (author != null)
            {
                Cheep newCheep = new Cheep
                {
                    Text = cheepDTO.Text,
                    Author =  author,
                    TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
                };

                // Add the new Cheep to the DbContext
                await _dbContext.Cheeps.AddAsync(newCheep);
            }

            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }

        // Update message (to be implemented)
        public Task UpdateCheep(CheepDTO alteredCheep)
        {
            throw new NotImplementedException();
        }
        // Find The author by name
        public AuthorDTO? FindAuthorByNameDTO(String name)
        {
            var author = (from a in _dbContext.Authors
                where a.Name == name
                select new AuthorDTO()
                {
                    Name = a.Name,
                    Email = a.Email,
                    AuthorsFollowed = a.AuthorsFollowed
                }).FirstOrDefault();
            
            return author;
        }
        
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
        public async Task CreateAuthor(string authorName, string authorEmail)
        {
            var author = new Author()
            {   
                Name = authorName,
                Email = authorEmail,
                Cheeps = new List<Cheep>(),
                AuthorsFollowed = new List<string>()
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
        
        public async Task DeleteUserCheeps(AuthorDTO Author)
        {
            var cheeps = await _dbContext.Cheeps
                .Where(cheep => cheep.Author.Name == Author.Name)
                .ToListAsync();
            if (cheeps != null)
            {
                _dbContext.Cheeps.RemoveRange(cheeps);

                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteUser(AuthorDTO Author)
        {
            // Retrieve the author by name and then remove them
            var author = await _dbContext.Authors
                .FirstOrDefaultAsync(a => a.Name == Author.Name);

            if (author != null)
            {
                _dbContext.Authors.Remove(author);
            }

            await _dbContext.SaveChangesAsync();
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
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString()
                });
            // Execute the query and return the list of messages
            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Follows an author
        /// </summary>
        /// <param name="userAuthorName"></param>
        /// <param name="followedAuthorName"></param>
        public async Task FollowAuthor(string userAuthorName, string followedAuthorName)
        {
            var author = FindAuthorByName(userAuthorName); // Find the author by name
            if (author != null && !author.AuthorsFollowed.Contains(followedAuthorName)) // Check if the author is not already followed
            {
                author.AuthorsFollowed.Add(followedAuthorName); // Add the author to the list of followed authors
                await _dbContext.SaveChangesAsync(); // Persist the changes to the database
            }
        }

        /// <summary>
        /// Unfollows an author
        /// </summary>
        /// <param name="userAuthorName"></param>
        /// <param name="authorToBeRemoved"></param>
        public async Task UnfollowAuthor(string userAuthorName, string authorToBeRemoved)
        {
            var author = FindAuthorByName(userAuthorName); // Find the author by name
            if (author != null && author.AuthorsFollowed.Contains(authorToBeRemoved)) // Check if the author is followed
            {
                author.AuthorsFollowed.Remove(authorToBeRemoved); // Remove the author from the list of followed authors
                await _dbContext.SaveChangesAsync(); // Persist the changes to the database
            }
        }

        /// <summary>
        /// When deleting user data we need to delete the username for every other author list. 
        /// </summary>
        /// <param name="authorName"></param>
        /// <param name="page"></param>
        public async Task RemovedAuthorFromFollowingList(string authorName)
        {
            // Fetch all authors that follows a specific author 
            var authors = await _dbContext.Authors
                .Where(author => author.AuthorsFollowed.Contains(authorName))
                .ToListAsync();
            
            // Remove the specific author from the list for all authors
            foreach (var author in authors) 
            {
                author.AuthorsFollowed.Remove(authorName);
            }
            
            await _dbContext.SaveChangesAsync();
        }
    }
}
