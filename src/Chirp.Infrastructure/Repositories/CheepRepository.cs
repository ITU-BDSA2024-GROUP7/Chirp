using System.Globalization;
using Chirp.Core;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
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

        /// <summary>
        /// Reads cheeps from a specific author and maps them to CheepDTO.
        /// </summary>
        /// <param name="userName">The name of the author.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <returns>A list of CheepDTO objects.</returns>
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
                        AuthorsFollowed = cheep.Author.AuthorsFollowed,
                        ProfilePicture = cheep.Author.ProfilePicture
                    },
                    Text = cheep.Text,
                    ImageReference = cheep.ImageReference ?? "",
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });

            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Retrieves all cheeps from a specific author.
        /// </summary>
        /// <param name="Username">The name of the author.</param>
        /// <returns>A list of CheepDTO objects.</returns>
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
                        AuthorsFollowed = cheep.Author.AuthorsFollowed,
                        ProfilePicture = cheep.Author.ProfilePicture
                    },
                    Text = cheep.Text,
                    ImageReference = cheep.ImageReference ?? "",
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });

            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Retrieves all comments from a specific author.
        /// </summary>
        /// <param name="Username">The name of the author.</param>
        /// <returns>A list of CommentDTO objects.</returns>
        public async Task<List<CommentDTO>> RetriveAllCommentsFromAnAuthor(string Username)
        {
            var query = _dbContext.Comment
                .Include(c => c.Author)
                .Where(comment => comment.Author.Name == Username)
                .OrderByDescending(comment => comment.TimeStamp)
                .Select(comment => new CommentDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = comment.Author.Name,
                        Email = comment.Author.Email,
                        AuthorsFollowed = comment.Author.AuthorsFollowed
                    },
                    CommentId = comment.CommentId,
                    CheepId = comment.CheepId,
                    Text = comment.Text,
                    FormattedTimeStamp = comment.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                });

            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Reads all cheeps with pagination.
        /// </summary>
        /// <param name="page">The page number for pagination.</param>
        /// <returns>A list of CheepDTO objects.</returns>
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
                        AuthorsFollowed = cheep.Author.AuthorsFollowed,
                        ProfilePicture = cheep.Author.ProfilePicture
                    },
                    Text = cheep.Text,
                    ImageReference = cheep.ImageReference ?? "",
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });

            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Reads private cheeps for a specific user with pagination.
        /// </summary>
        /// <param name="page">The page number for pagination.</param>
        /// <param name="userName">The name of the user.</param>
        /// <returns>A list of CheepDTO objects.</returns>
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
                        AuthorsFollowed = cheep.Author.AuthorsFollowed,
                        ProfilePicture = cheep.Author.ProfilePicture
                    },
                    Text = cheep.Text,
                    ImageReference = cheep.ImageReference ?? "",
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets the total number of pages for cheeps.
        /// </summary>
        /// <param name="authorName">The name of the author (optional).</param>
        /// <returns>The total number of pages.</returns>
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
        
        /// <summary>
        /// Gets the total number of pages for popular cheeps.
        /// </summary>
        /// <returns>The total number of pages.</returns>
        public async Task<int> GetTotalPageNumberForPopular()
        {
            var query = _dbContext.Cheeps
                .Include(c => c.Author)
                .Where(cheep => cheep.Likes.Count > 0)
                .OrderByDescending(cheep => cheep.Likes.Count)
                .Select(cheep => new CheepDTO
                {
                    CheepId = cheep.CheepId,
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed,
                        ProfilePicture = cheep.Author.ProfilePicture
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });


            var totalCheeps = await query.CountAsync();

            return (int)Math.Ceiling((double)totalCheeps / 32); // Math.Ceiling (round up) to ensure all pages
        }

        /// <summary>
        /// Creates a new cheep.
        /// </summary>
        /// <param name="cheepDTO">The CheepDTO object containing cheep details.</param>
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
                    ImageReference = cheepDTO.ImageReference,
                    TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
                };

                // Add the new Cheep to the DbContext
                await _dbContext.Cheeps.AddAsync(newCheep);
            }

            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }
        
        /// <summary>
        /// Updates an existing cheep.
        /// </summary>
        /// <param name="alteredCheep">The CheepDTO object containing updated cheep details.</param>
        public Task UpdateCheep(CheepDTO alteredCheep)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Deletes a cheep by its ID.
        /// </summary>
        /// <param name="cheepId">The ID of the cheep to delete.</param>
        public async Task DeleteCheep(int cheepId)
        {
            var cheep = await _dbContext.Cheeps.FindAsync(cheepId);
            if (cheep != null)
            {
                _dbContext.Cheeps.Remove(cheep);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// Deletes all cheeps from a specific author.
        /// </summary>
        /// <param name="Author">The AuthorDTO object containing author details.</param>
        public async Task DeleteUserCheeps(AuthorDTO Author)
        {
            var cheeps = await _dbContext.Cheeps
                .Where(cheep => cheep.Author.Name == Author.Name)
                .ToListAsync();
            if (!cheeps.Count.Equals(0))
            {
                _dbContext.Cheeps.RemoveRange(cheeps);

                await _dbContext.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// Retrieves all cheeps for an endpoint.
        /// </summary>
        /// <returns>A list of CheepDTO objects.</returns>
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
                        AuthorsFollowed = cheep.Author.AuthorsFollowed,
                        ProfilePicture = cheep.Author.ProfilePicture
                    },
                    Text = cheep.Text,
                    FormattedTimeStamp = cheep.TimeStamp.ToString(CultureInfo.CurrentCulture)
                });
            // Execute the query and return the list of messages
            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Likes a cheep.
        /// </summary>
        /// <param name="authorId">The ID of the author.</param>
        /// <param name="cheepId">The ID of the cheep to like.</param>
        public async Task HandleLike(string authorName, int cheepId, string? emoji = null)
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
                if (emoji != null)
                {
                    await UpdateReaction(cheepId, authorName, emoji);
                }
                else
                {
                    await UnlikeCheep(existingLike);
                    await RemoveReaction(cheepId, authorName); // Remove the reaction when unliking
                }
            }
            else // if the user has not liked the cheep
            {
                if (existingDislike != null)
                {
                    // undislike cheep if the user has disliked it
                    await UnDislikeCheep(existingDislike);
                    await RemoveReaction(cheepId, authorName); // Remove the dislike reaction when unliking
                }

                await LikeCheep(authorId, cheepId);
                if (emoji != null)
                {
                    await AddReaction(cheepId, authorName, emoji); // Add reaction when liking
                }
            }
        }
        
        /// <summary>
        /// Likes a cheep.
        /// </summary>
        /// <param name="authorId">The ID of the author.</param>
        /// <param name="cheepId">The ID of the cheep to like.</param>
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
        /// <summary>
        /// Unlikes a cheep.
        /// </summary>
        /// <param name="existingLike">The existing Like object to remove.</param>
        public async Task UnlikeCheep(Like existingLike)
        {
            // Remove the Like from the DbContext
            _dbContext.Likes.Remove(existingLike);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Handles disliking a cheep.
        /// </summary>
        /// <param name="authorName">The name of the author.</param>
        /// <param name="cheepId">The ID of the cheep to dislike.</param>
        /// <param name="emoji">The emoji reaction (optional).</param>
        public async Task HandleDislike(string authorName, int cheepId, string? emoji = null)
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
                if (emoji != null)
                {
                    await UpdateReaction(cheepId, authorName, emoji);
                }
                else
                {
                    await UnDislikeCheep(existingDislike);
                    await RemoveReaction(cheepId, authorName); // Remove the reaction when unliking
                }
            }
            else // undislike cheep if the user has disliked it
            {
                if (existingLike != null)
                {
                    await UnlikeCheep(existingLike);
                    await RemoveReaction(cheepId, authorName); // Remove the reaction when unliking
                }

                // if the user has not disliked the cheep
                await DislikeCheep(authorId, cheepId);
                if (emoji != null)
                {
                    await AddReaction(cheepId, authorName, emoji); // Add reaction when disliking
                }
            }
        }
        
        /// <summary> 
        /// Dislikes a cheep.
        /// </summary>
        /// <param name="authorId">The ID of the author.</param>
        /// <param name="cheepId">The ID of the cheep to dislike.</param>
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
        /// <summary>
        /// Undislikes a cheep.
        /// </summary>
        /// <param name="existingLike">The existing Dislike object to remove.</param>
        public async Task UnDislikeCheep(Dislike existingLike)
        {
            // Remove the Dislike from the DbContext
            _dbContext.Dislikes.Remove(existingLike);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Gets popular cheeps with pagination.
        /// </summary>
        /// <param name="page">The page number for pagination.</param>
        /// <returns>A list of CheepDTO objects.</returns>
        public async Task<List<CheepDTO>> GetPopularCheeps(int page)
        {
            var query = _dbContext.Cheeps
                .Include(c => c.Author)
                .Where(cheep => cheep.Likes.Count > 0)
                .OrderByDescending(cheep => cheep.Likes.Count)
                .Skip((page - 1) * 32)
                .Take(32)
                .Select(cheep => new CheepDTO
                {
                    CheepId = cheep.CheepId,
                    Author = new AuthorDTO
                    {
                        Name = cheep.Author.Name,
                        Email = cheep.Author.Email,
                        AuthorsFollowed = cheep.Author.AuthorsFollowed,
                        ProfilePicture = cheep.Author.ProfilePicture
                    },
                    Text = cheep.Text,
                    ImageReference = cheep.ImageReference ?? "",
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });

            return await query.ToListAsync();
        }

        /// <summary>
        /// Adds a reaction to a cheep.
        /// </summary>
        /// <param name="cheepId">The ID of the cheep.</param>
        /// <param name="authorName">The name of the author.</param>
        /// <param name="emoji">The emoji reaction.</param>
        public async Task AddReaction(int cheepId, string authorName, string emoji)
        {
            // Find the author by Id (or by name if needed)
            var author = await _authorRepository.FindAuthorByName(authorName);
            var authorId = author!.AuthorId;
            if (author == null)
            {
                // Handle case where author is not found
                throw new Exception("Author not found");
            }

            var reaction = new Reaction
            {
                CheepId = cheepId,
                AuthorId = authorId,
                Emoji = emoji
            };

            await _dbContext.Reaction.AddAsync(reaction);
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Removes a reaction from a cheep.
        /// </summary>
        /// <param name="cheepId">The ID of the cheep.</param>
        /// <param name="authorName">The name of the author.</param>
        public async Task RemoveReaction(int cheepId, string authorName)
        {
            var author = await _authorRepository.FindAuthorByName(authorName);
            var authorId = author!.AuthorId;
            if (author == null)
            {
                // Handle case where author is not found
                throw new Exception("Author not found");
            }

            var reaction = await _dbContext.Reaction
                .FirstOrDefaultAsync(r => r.CheepId == cheepId && r.AuthorId == authorId);

            if (reaction != null)
            {
                _dbContext.Reaction.Remove(reaction);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// Gets the top reactions for a cheep.
        /// </summary>
        /// <param name="cheepId">The ID of the cheep.</param>
        /// <param name="topN">The number of top reactions to retrieve.</param>
        /// <returns>A list of top emoji reactions.</returns>
        public async Task<List<String>> GetTopReactions(int cheepId, int topN = 3)
        {
            return await _dbContext.Reaction
                .Where(r => r.CheepId == cheepId)
                .GroupBy(r => r.Emoji)
                .OrderByDescending(g => g.Count())
                .Take(topN)
                .Select(g => g.First().Emoji)
                .ToListAsync();
        }
        
        /// <summary>
        /// Updates a reaction for a cheep.
        /// </summary>
        /// <param name="cheepId">The ID of the cheep.</param>
        /// <param name="authorName">The name of the author.</param>
        /// <param name="emoji">The new emoji reaction.</param>
        public async Task UpdateReaction(int cheepId, string authorName, string emoji)
        {
            var author = await _authorRepository.FindAuthorByName(authorName);
            var authorId = author!.AuthorId;
            if (author == null)
            {
                throw new Exception("Author not found");
            }

            var reaction = await _dbContext.Reaction
                .FirstOrDefaultAsync(r => r.CheepId == cheepId && r.AuthorId == authorId);

            if (reaction != null)
            {
                reaction.Emoji = emoji;
                _dbContext.Reaction.Update(reaction);
                await _dbContext.SaveChangesAsync();
            } else if (reaction == null)
            {
                await AddReaction(cheepId, authorName, emoji);
            }
        }
        
        /// <summary>
        /// Handles image upload and compression.
        /// </summary>
        /// <param name="image">The image file to upload.</param>
        /// <returns>The base64 string of the compressed image.</returns>
        public async Task<string> HandleImageUpload(IFormFile image)
        {
            // Check if the file is a GIF
            if(image.ContentType == "image/gif")
            {
                var compressedGIF = await CompressGIF(image);
                return Convert.ToBase64String(compressedGIF);
            }
            
            // Compress the image and get the byte array
            var compressedImageBytes = await CompressImage(image);
    
            // Convert the byte array to base64 string
            var base64String = Convert.ToBase64String(compressedImageBytes);
            return base64String;
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
                return null; // Or throw an exception if you want to handle this case
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
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(1024, 1024) // Max size (width x height)
            }));

            // Step 4: Save the processed image to a memory stream (compressed with quality)
            using var outputStream = new MemoryStream();
            if (image.ContentType == "image/png")
            {
                img.Save(outputStream, new WebpEncoder
                {
                    Quality = 30, // Adjust quality as needed (0-100 scale)
                    FileFormat = WebpFileFormatType.Lossy // Use lossy compression
                });
            }
            else
            {
                img.Save(outputStream, new JpegEncoder
                {
                    Quality = 30  // Adjust quality as needed (0-100 scale)


                });
            }

            outputStream.Position = 0; // Reset position to the start of the stream

            // Step 5: Return the byte array of the compressed image
            return outputStream.ToArray();
        }

        /// <summary>
        /// Compresses a GIF file.
        /// </summary>
        /// <param name="gifFile">The GIF file to compress.</param>
        /// <returns>The byte array of the compressed GIF.</returns>
        public async Task<byte[]> CompressGIF(IFormFile gifFile)
        {
            if (gifFile == null || gifFile.Length == 0)
                throw new ArgumentException("No file uploaded.");

            using (var stream = gifFile.OpenReadStream())
            {
                using (var imageCollection = new MagickImageCollection(stream))
                {
                    // Determine the size to resize all frames to
                    uint maxWidth = 500;
                    uint maxHeight = 500;

                    // Resize the entire collection to ensure consistent frame sizes
                    imageCollection.Coalesce();
                    foreach (var frame in imageCollection)
                    {
                        if (frame.Width > maxWidth || frame.Height > maxHeight)
                        {
                            frame.Resize(new MagickGeometry(maxWidth, maxHeight)
                            {
                                IgnoreAspectRatio = false // Maintain aspect ratio
                            });
                        }
                        frame.Quality = 30; 
                    }

                    // Create a memory stream to hold the compressed GIF
                    using (var memoryStream = new MemoryStream())
                    {
                        imageCollection.Write(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Gets comments by cheep ID.
        /// </summary>
        /// <param name="cheepId">The ID of the cheep.</param>
        /// <returns>A list of CommentDTO objects.</returns>
        public async Task<List<CommentDTO>> GetCommentsByCheepId(int cheepId)
        {
            var query = _dbContext.Comment
                .Where(comment => comment.CheepId == cheepId)
                .OrderByDescending(comment => comment.TimeStamp)
                .Select(comment => new CommentDTO
                {
                    Author = new AuthorDTO
                    {
                        Name = comment.Author.Name,
                        Email = comment.Author.Email,
                        AuthorsFollowed = comment.Author.AuthorsFollowed
                    },
                    CommentId = comment.CommentId,
                    CheepId = comment.CheepId,
                    Text = comment.Text,
                    FormattedTimeStamp = comment.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                });
            
            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Adds a comment to a cheep.
        /// </summary>
        /// <param name="cheepDto">The CheepDTO object containing cheep details.</param>
        /// <param name="text">The text of the comment.</param>
        /// <param name="author">The name of the author.</param>
        public async Task AddCommentToCheep(CheepDTO cheepDto, string text, string author )
        {
                var comment = new Comment
                {
                    CommentId = 0,
                    CheepId = cheepDto.CheepId,
                    Author = await _authorRepository.FindAuthorByName(author),
                    Text = text,
                    TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
                    
                };
            
                // Add the new Cheep to the DbContext
                await _dbContext.Comment.AddAsync(comment);

            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }
        
        /// <summary>
        /// Gets a cheep by its ID.
        /// </summary>
        /// <param name="cheepId">The ID of the cheep.</param>
        /// <returns>The CheepDTO object.</returns>
        public async Task<CheepDTO> GetCheepFromId(int cheepId)
        {
            var cheep = await (from a in _dbContext.Cheeps
                where a.CheepId == cheepId
                select new CheepDTO()
                {
                    CheepId = a.CheepId,
                    Author = new AuthorDTO
                    {
                        Name = a.Author.Name,
                        Email = a.Author.Email,
                        AuthorsFollowed = a.Author.AuthorsFollowed
                    }, 
                    Text = a.Text,
                    FormattedTimeStamp = a.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = a.Likes,
                    Dislikes = a.Dislikes,
                    LikesCount = a.Likes.Count,
                    DislikesCount = a.Dislikes.Count,
                    ImageReference = a.ImageReference
                }).FirstOrDefaultAsync();

            return cheep ?? throw new InvalidOperationException();
        }
        
        /// <summary>
        /// Deletes a comment by its ID.
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete.</param>
        public async Task DeleteComment(int commentId)
        {
            var comment = await _dbContext.Comment.FindAsync(commentId);
            if (comment != null)
            {
                _dbContext.Comment.Remove(comment);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}