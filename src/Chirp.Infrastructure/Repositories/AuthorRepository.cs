using Chirp.Core;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using CheepDTO = Chirp.Core.DTOs.CheepDTO;

namespace Chirp.Infrastructure.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly CheepDBContext _dbContext;

        public AuthorRepository(CheepDBContext dbContext)
        {
            _dbContext = dbContext;
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
