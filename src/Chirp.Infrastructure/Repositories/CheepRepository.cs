using Chirp.Core;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
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
                    ImageReference = cheep.ImageReference,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
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
                    ImageReference = cheep.ImageReference,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });

            return await query.ToListAsync();
        }
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
                    ImageReference = cheep.ImageReference,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
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
                    ImageReference = cheep.ImageReference,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
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
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
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
                    ImageReference = cheepDTO.ImageReference,
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
                        AuthorsFollowed = cheep.Author.AuthorsFollowed
                    },
                    Text = cheep.Text,
                    ImageReference = cheep.ImageReference,
                    FormattedTimeStamp = cheep.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    Likes = cheep.Likes,
                    Dislikes = cheep.Dislikes,
                    LikesCount = cheep.Likes.Count,
                    DislikesCount = cheep.Dislikes.Count,
                    CommentsCount = cheep.Comments.Count
                });

            return await query.ToListAsync();
        }


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

        public async Task RemoveReaction(int cheepId, string authorName)
        {
            // Find the author by Id (or by name if needed)
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
            img.Save(outputStream, new JpegEncoder
            {
                Quality = 75  // Adjust quality as needed (0-100 scale)
            });

            outputStream.Position = 0; // Reset position to the start of the stream

            // Step 5: Return the byte array of the compressed image
            return outputStream.ToArray();
        }
        
        public async Task<byte[]> CompressGIF(IFormFile gif)
        {
            if (gif == null || gif.Length == 0)
            {
                return null; // Or throw an exception
            }

            using var inputStream = new MemoryStream();
            await gif.CopyToAsync(inputStream);
            inputStream.Position = 0; // Reset the stream position

            // Step 1: Load the GIF using MagickImageCollection for animations
            using var gifCollection = new MagickImageCollection(inputStream);

            // Step 2: Optimize the GIF frames
            var resize = false;
            if (gifCollection[0].Width >= 1024 || gifCollection[0].Height >= 1024)
            {
                resize = true;
            } 
            foreach (var frame in gifCollection)
            {
                if (resize)
                {
                    frame.Resize(1024, 1024); // Resize to max dimensions
                }
                frame.Strip(); // Remove unnecessary metadata
                frame.Quantize(new QuantizeSettings
                {
                    Colors = 128 // Limit the number of colors to control size
                });
            }
            
            
            // Reduce file size by optimizing color palette and frames
            gifCollection.Optimize();

            // Step 3: Write the optimized GIF to a memory stream
            using var outputStream = new MemoryStream();
            gifCollection.Write(outputStream);

            // Step 4: Return the byte array of the compressed GIF
            return outputStream.ToArray();
        }

            
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
        
        public async Task AddCommentToCheep(CheepDTO cheepDto, string Text, string author )
        {
            // Create a new Cheep 
            if (cheepDto != null)
            {
                Comment comment = new Comment
                {
                    CommentId = 0,
                    CheepId = cheepDto.CheepId,
                    Author = await _authorRepository.FindAuthorByName(author),
                    Text = Text,
                    TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"))
                    
                };
                
                // Add the new Cheep to the DbContext
                await _dbContext.Comment.AddAsync(comment);
            }

            await _dbContext.SaveChangesAsync(); // Persist the changes to the database
        }

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
                    DislikesCount = a.Dislikes.Count
                }).FirstOrDefaultAsync();

            return cheep;
        }
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