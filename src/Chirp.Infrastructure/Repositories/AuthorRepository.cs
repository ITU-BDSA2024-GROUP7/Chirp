using Chirp.Core;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
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
        /// <summary>
        /// Finds an author in the database using the name of the author
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<AuthorDTO?> FindAuthorByNameDTO(string name)
        {
            var author = await (from a in _dbContext.Authors
                where a.Name == name
                select new AuthorDTO
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name,
                    Email = a.Email,
                    AuthorsFollowed = a.AuthorsFollowed,
                    ProfilePicture = a.ProfilePicture
                }).FirstOrDefaultAsync();

            return author;
        }
        /// <summary>
        ///  This method finds a given author based on the name
        /// </summary>
        /// <param name="name"></param>
        public async Task<Author?> FindAuthorByName(string name)
        {
            return await _dbContext.Authors
                .FirstOrDefaultAsync(a => a.Name == name);
        }
        
        // Used for creating a new author when the author is not existing
        
        /// <summary>
        ///  This method is for creating a new author
        /// </summary>
        /// <param name="authorName"></param>
        /// <param name="authorEmail"></param>
        /// <param name="profilePicture"></param>
        public async Task CreateAuthor(string authorName, string authorEmail, string profilePicture)
        {
            string? base64ProfilePicture = null;
            if (profilePicture != null)
            {
                base64ProfilePicture = await DownloadAndConvertToBase64Async(profilePicture);
            }
            
            var author = new Author()
            {   
                Name = authorName,
                Email = authorEmail,
                Cheeps = new List<Cheep>(),
                AuthorsFollowed = new List<string>(),
                ProfilePicture = base64ProfilePicture
            };
            await _dbContext.Authors.AddAsync(author);
            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }
        /// <summary>
        /// This method is used for deleting an author
        /// </summary>
        /// <param name="Author"></param>
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
        /// This Method is for Following an author
        /// </summary>
        /// <param name="userAuthorName"></param>
        /// <param name="followedAuthorName"></param>
        public async Task FollowAuthor(string userAuthorName, string followedAuthorName)
        {
            var author = await FindAuthorByName(userAuthorName); // Find the author by name
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
            var author = await FindAuthorByName(userAuthorName); // Find the author by name
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
        
        /// <summary>
        /// Returns the list of authors that a user follows
        /// </summary>
        /// <param name="userName">The username from the url</param>
        /// <returns></returns>
        public async Task<List<string>> GetFollowedAuthors(string userName)
        {
            var author = await Task.Run(() => FindAuthorByName(userName));
            return author?.AuthorsFollowed.ToList() ?? new List<string>();
        }
        
        /// <summary>
        /// Returns the list of authors that follows a user
        /// </summary>
        /// <param name="userName">The username from the url</param>
        /// <returns></returns>
        public async Task<List<string>> GetFollowingAuthors(string userName)
        {
            var author = await Task.Run(() => FindAuthorByName(userName));
            var followingAuthors = new List<string>();
            foreach (Author a in _dbContext.Authors) 
            {
                if (a.AuthorsFollowed.Contains(userName))
                {
                    followingAuthors.Add(a.Name);
                }
            }

            return followingAuthors;
        }
        /// <summary>
        /// This methods returns the amount of karma a specific user has
        /// </summary>
        /// <param name="authorName"></param>
        /// <returns>Karma</returns>
        public async Task<int> GetKarmaForAuthor(string authorName)
        {
            var author = await FindAuthorByName(authorName);

            // Check for if author is null
            if (author == null) return 0;
            
            var cheepIds = await _dbContext.Cheeps
                .Where(cheep => cheep.AuthorId == author.AuthorId)
                .Select(cheep => cheep.CheepId)
                .ToListAsync();

            var likesCount = await _dbContext.Likes
                .CountAsync(like => cheepIds.Contains(like.CheepId));

            var dislikesCount = await _dbContext.Dislikes
                .CountAsync(dislike => cheepIds.Contains(dislike.CheepId));

            var karma = likesCount - dislikesCount;
            return karma;
        }
        /// <summary>
        /// Downloads an image from a given URL and converts it to a Base64 string.
        /// </summary>
        /// <param name="imageUrl">The URL of the image to download.</param>
        /// <returns>A Base64 string representation of the downloaded image.</returns>
        /// <exception cref="Exception">Thrown when there is an error downloading or converting the image.</exception>
        public async Task<string> DownloadAndConvertToBase64Async(string imageUrl)
        {
            // Create an HTTP client
            using var httpClient = new HttpClient();

            try
            {
                // Download the image as a byte array
                byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                // Convert the byte array to a Base64 string
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading or converting image: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Method for updating your profile picture
        /// </summary>
        /// <param name="authorName"></param>
        /// <param name="profilePicture"></param>
        public async Task UpdateProfilePicture(string authorName, IFormFile profilePicture)
        {
            var author = await FindAuthorByName(authorName);
            if (author != null)
            {
                var compressedImage = await CompressImage(profilePicture);
                var base64Image = Convert.ToBase64String(compressedImage);
                author.ProfilePicture = base64Image;
                await _dbContext.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Compresses an image file.
        /// </summary>
        /// <param name="image">The image file to compress.</param>
        /// <returns>The byte array of the compressed image.</returns>
        public async Task<byte[]> CompressImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            // Step 1: Convert IFormFile to MemoryStream
            using var inputStream = new MemoryStream();
            await image.CopyToAsync(inputStream);
            inputStream.Position = 0; // Reset the position to the start of the stream after copying

            // Step 2: Load the image using ImageSharp
            using var img = Image.Load(inputStream);

            // Step 3: Resize the image (max width/height) and compress it
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(500, 500),
                Position = AnchorPositionMode.Center // Crop from the center
            }));

            // Step 4: Save the processed image to a memory stream (compressed with quality)
            using var outputStream = new MemoryStream();
            img.Save(outputStream, new JpegEncoder
            {
                Quality = 30  // Adjust quality as needed (0-100 scale)
            });

            outputStream.Position = 0; // Reset position to the start of the stream

            // Step 5: Return the byte array of the compressed image
            return outputStream.ToArray();
        }
        /// <summary>
        /// Method for clearing a users profilepicture
        /// </summary>
        /// <param name="authorName"></param>
        /// <param name="profilePicture"></param>
        public async Task ClearProfilePicture(string authorName, IFormFile profilePicture)
        {
            var author = await FindAuthorByName(authorName);
            if (author != null)
            {
                author.ProfilePicture = null;
                await _dbContext.SaveChangesAsync();
            }
        }
    }    
}
