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
        private readonly AuthorRepository _authorRepository;

        public CheepRepository(CheepDBContext dbContext, AuthorRepository authorRepository)
        {
            _dbContext = dbContext;
            _authorRepository = authorRepository;
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
                    CheepId = cheep.CheepId,
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count
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
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count
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
                    CheepId = cheep.CheepId,
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count
                });

            return await query.ToListAsync();
        }

        public async Task<List<CheepDTO>> ReadPrivateCheeps(int page, string userName)
        {
            // Resolve the user and their followed authors first
            var userAuthor = await _authorRepository.FindAuthorByName(userName);
            if (userAuthor == null)
            {
                // Return an empty list if the user is not found
                return new List<CheepDTO>();
            }
            
            // Get the list of authors followed by the user
            var followedAuthors = userAuthor.AuthorsFollowed;
            
            var query = _dbContext.Cheeps
                .Include(c => c.Author) 
                .Where(cheep => followedAuthors.Contains(cheep.Author.Name) || cheep.Author.Name == userName)
                .OrderByDescending(cheep => cheep.TimeStamp)
                .Skip((page - 1) * 32)
                .Take(32)
                .Select(cheep => new CheepDTO
                {
                    CheepId = cheep.CheepId,
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count
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
            var author = await _authorRepository.FindAuthorByName(cheepDTO.Author.Name);
            
            // Create a new Cheep 
            if (author != null)
            {
                Cheep newCheep = new Cheep
                {
                    Text = cheepDTO.Text,
                    Author = author,
                    TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
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

        public async Task HandleLike(string authorName, int cheepId)
        {
            // Find the author by Id (or by name if needed)
            var author = await _authorRepository.FindAuthorByName(authorName);
            var authorId = author!.AuthorId;
            if (author == null)
            {
                // Handle case where author is not found
                throw new Exception("Author not found");
            }

            // Find the Cheep by its Id
            var cheep = await _dbContext.Cheeps.FindAsync(cheepId);
            if (cheep == null)
            {
                // Handle case where cheep is not found
                throw new Exception("Cheep not found");
            }
            
            // Check if the author has disliked this cheep
            var existingDislike = await _dbContext.Dislikes
                .FirstOrDefaultAsync(dl => dl.CheepId == cheepId && dl.AuthorId == authorId);
            
            // Check if the author has already liked this cheep
            var existingLike = await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.CheepId == cheepId && l.AuthorId == authorId);

            
            if (existingLike != null) // if the user has already liked the cheep
            {
                
                await UnlikeCheep(existingLike);
            }
            else // if the user has not liked the cheep
            {
                if (existingDislike != null)
                {
                    // undislike cheep if the user has disliked it
                    await UnDislikeCheep(existingDislike);
                }
                
                await LikeCheep(authorId, cheepId);
            }
        }
        
        public async Task LikeCheep(int authorId, int cheepId)
        {
            
            // Create a new Like record
            var like = new Like
            {
                CheepId = cheepId,
                AuthorId = authorId
            };

            // Add the new Like to the DbContext
            await _dbContext.Likes.AddAsync(like);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnlikeCheep(Like existingLike)
        {
            // Remove the Like from the DbContext
            _dbContext.Likes.Remove(existingLike);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

        }

        public async Task HandleDislike(string authorName, int cheepId)
        {
            // Find the author by Id (or by name if needed)
            var author = await _authorRepository.FindAuthorByName(authorName);
            var authorId = author!.AuthorId;
            if (author == null)
            {
                // Handle case where author is not found
                throw new Exception("Author not found");
            }

            // Find the Cheep by its Id
            var cheep = await _dbContext.Cheeps.FindAsync(cheepId);
            if (cheep == null)
            {
                // Handle case where cheep is not found
                throw new Exception("Cheep not found");
            }
            // Check if the author has already liked this cheep
            var existingLike = await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.CheepId == cheepId && l.AuthorId == authorId);
            
            // Check if the author has already disliked this cheep
            var existingDislike = await _dbContext.Dislikes
                .FirstOrDefaultAsync(dl => dl.CheepId == cheepId && dl.AuthorId == authorId);

            if (existingDislike != null) // if the user has already disliked the cheep
            {
                await UnDislikeCheep(existingDislike);
            }
            else // undislike cheep if the user has disliked it
            {
                if (existingLike != null)
                {
                    await UnlikeCheep(existingLike);
                }
                // if the user has not disliked the cheep
                await DislikeCheep(authorId, cheepId);
            }
        }
        
        public async Task DislikeCheep(int authorId, int cheepId)
        {
            // Create a new Dislike record
            var dislike = new Dislike()
            {
                CheepId = cheepId,
                AuthorId = authorId
            };

            // Add the new Dislike to the DbContext
            await _dbContext.Dislikes.AddAsync(dislike);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnDislikeCheep(Dislike existingLike)
        {
            // Remove the Dislike from the DbContext
            _dbContext.Dislikes.Remove(existingLike);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }
    }
}
