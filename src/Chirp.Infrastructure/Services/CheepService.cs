using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Chirp.Infrastructure.Services;

public class CheepService : ICheepService
{
    private readonly CheepRepository _cheepRepository;
    private readonly AuthorRepository _authorRepository;
    public CheepService(CheepRepository cheepRepository, AuthorRepository authorRepository)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
    }
    
    /// <summary>
    /// Gets the total number of pages for cheeps.
    /// </summary>
    /// <param name="authorName">The name of the author (optional).</param>
    /// <returns>The total number of pages.</returns>
    public async Task<int> GetTotalPageNumber(string authorName = "")
    {
        return await _cheepRepository.GetTotalPages(authorName);
    }
    
    /// <summary>
    /// Reads all cheeps with pagination.
    /// </summary>
    /// <param name="page">The page number for pagination.</param>
    /// <returns>A list of CheepDTO objects.</returns>
    public async Task<List<Core.DTOs.CheepDTO>> GetCheeps(int page)
    {
        return await _cheepRepository.ReadAllCheeps(page);
    }
    
    /// <summary>
    /// Reads cheeps from a specific author and maps them to CheepDTO.
    /// </summary>
    /// <param name="userName">The name of the author.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <returns>A list of CheepDTO objects.</returns>
    public async Task<List<Core.DTOs.CheepDTO>> GetCheepsFromAuthor(string author, int page)
    {
        return await _cheepRepository.ReadCheepsFromAuthor(author, page);
    }
    
    /// <summary>
    /// Reads private cheeps for a specific user with pagination.
    /// </summary>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="userName">The name of the user.</param>
    /// <returns>A list of CheepDTO objects.</returns>
    public async Task<List<Core.DTOs.CheepDTO>> GetPrivateCheeps(int page, string username)
    {
        return await _cheepRepository.ReadPrivateCheeps(page, username);
    }
    
    /// <summary>
    /// Retrieves all cheeps for an endpoint.
    /// </summary>
    /// <returns>A list of CheepDTO objects.</returns>
    public async Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheeps()
    {
        return await _cheepRepository.RetrieveAllCheepsForEndPoint();
    }
    
    /// <summary>
    /// Creates a new cheep.
    /// </summary>
    /// <param name="cheepDTO">The CheepDTO object containing cheep details.</param>
    public async Task CreateCheep(CheepDTO Cheep)
    {
        await _cheepRepository.CreateCheep(Cheep);
    }
    
    /// <summary>
    /// Deletes all cheeps from a specific author.
    /// </summary>
    /// <param name="Author">The AuthorDTO object containing author details.</param>
    public async Task DeleteUserCheeps(AuthorDTO Author)
    {
        await _cheepRepository.DeleteUserCheeps(Author);
    }
    
    /// <summary>
    /// Deletes a cheep by its ID.
    /// </summary>
    /// <param name="cheepId">The ID of the cheep to delete.</param>
    public async Task DeleteCheep(int cheepId)
    {
        await _cheepRepository.DeleteCheep(cheepId);
    }
    
    /// <summary>
    /// Deletes a comment by its ID.
    /// </summary>
    /// <param name="commentId">The ID of the comment to delete.</param>
    public async Task DeleteComment(int commentId)
    {
        await _cheepRepository.DeleteComment(commentId);
    }
    
    /// <summary>
    /// Retrieves all cheeps from a specific author.
    /// </summary>
    /// <param name="Username">The name of the author.</param>
    /// <returns>A list of CheepDTO objects.</returns>
    public async Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheepsFromAnAuthor(string authorName)
    {
        return await _cheepRepository.RetrieveAllCheepsFromAnAuthor(authorName);
    }

    /// <summary>
    /// Likes a cheep.
    /// </summary>
    /// <param name="authorId">The ID of the author.</param>
    /// <param name="cheepId">The ID of the cheep to like.</param>
    public async Task HandleLike(string authorName, int cheepId, string? emoji)
    {
        await _cheepRepository.HandleLike(authorName, cheepId, emoji);
    }
    
    /// <summary>
    /// Handles disliking a cheep.
    /// </summary>
    /// <param name="authorName">The name of the author.</param>
    /// <param name="cheepId">The ID of the cheep to dislike.</param>
    /// <param name="emoji">The emoji reaction (optional).</param>
    public async Task HandleDislike(string authorName, int cheepId, string? emoji)
    {
        await _cheepRepository.HandleDislike(authorName, cheepId, emoji);

    }
    
    /// <summary>
    /// Gets popular cheeps with pagination.
    /// </summary>
    /// <param name="page">The page number for pagination.</param>
    /// <returns>A list of CheepDTO objects.</returns>
    public async Task<List<CheepDTO>> GetPopularCheeps(int page)
    {
        return await _cheepRepository.GetPopularCheeps(page);
    }
    
    /// <summary>
    /// Gets the total number of pages for popular cheeps.
    /// </summary>
    /// <returns>The total number of pages.</returns>
    public async Task<int> GetTotalPageNumberForPopular()
    {
        return await _cheepRepository.GetTotalPageNumberForPopular();
    }
    
    /// <summary>
    /// Handles image upload and compression.
    /// </summary>
    /// <param name="image">The image file to upload.</param>
    /// <returns>The base64 string of the compressed image.</returns>
    public async Task<string> HandleImageUpload(IFormFile image)
    {
        return await _cheepRepository.HandleImageUpload(image);
    }
    
    /// <summary>
    /// Gets comments by cheep ID.
    /// </summary>
    /// <param name="cheepId">The ID of the cheep.</param>
    /// <returns>A list of CommentDTO objects.</returns>
    public async Task<List<CommentDTO>> GetCommentsByCheepId(int cheepId)
    {
        return await _cheepRepository.GetCommentsByCheepId(cheepId);
    }
    
    /// <summary>
    /// Adds a comment to a cheep.
    /// </summary>
    /// <param name="cheepDto">The CheepDTO object containing cheep details.</param>
    /// <param name="text">The text of the comment.</param>
    /// <param name="author">The name of the author.</param>
    public async Task AddCommentToCheep(CheepDTO cheepDto, string Text, string author)
    {
        await _cheepRepository.AddCommentToCheep(cheepDto, Text, author);
    }
    
    /// <summary>
    /// Gets a cheep by its ID.
    /// </summary>
    /// <param name="cheepId">The ID of the cheep.</param>
    /// <returns>The CheepDTO object.</returns>
    public async Task<CheepDTO?> GetCheepFromId(int cheepId)
    {
        return await _cheepRepository.GetCheepFromId(cheepId);
    }
    
    /// <summary>
    /// Gets the top reactions for a cheep.
    /// </summary>
    /// <param name="cheepId">The ID of the cheep.</param>
    /// <param name="topN">The number of top reactions to retrieve.</param>
    /// <returns>A list of top emoji reactions.</returns>
    public async Task<List<String>> GetTopReactions (int cheepId)
    {
        return await _cheepRepository.GetTopReactions(cheepId);
    }
    
    /// <summary>
    /// Retrieves all comments from a specific author.
    /// </summary>
    /// <param name="Username">The name of the author.</param>
    /// <returns>A list of CommentDTO objects.</returns>
    public async Task<List<Core.DTOs.CommentDTO>> RetrieveAllCommentsFromAnAuthor(string authorName)
    {
        return await _cheepRepository.RetriveAllCommentsFromAnAuthor(authorName);
    }
}