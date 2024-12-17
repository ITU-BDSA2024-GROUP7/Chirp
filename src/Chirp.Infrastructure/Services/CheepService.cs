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
    
    public async Task<int> GetTotalPageNumber(string authorName = "")
    {
        return await _cheepRepository.GetTotalPages(authorName);
    }
    public async Task<List<Core.DTOs.CheepDTO>> GetCheeps(int page)
    {
        return await _cheepRepository.ReadAllCheeps(page);
    }
    
    public async Task<List<Core.DTOs.CheepDTO>> GetCheepsFromAuthor(string author, int page)
    {
        return await _cheepRepository.ReadCheepsFromAuthor(author, page);
    }

    public async Task<List<Core.DTOs.CheepDTO>> GetPrivateCheeps(int page, string username)
    {
        return await _cheepRepository.ReadPrivateCheeps(page, username);
    }
    
    public async Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheeps()
    {
        return await _cheepRepository.RetrieveAllCheepsForEndPoint();
    }
    
    public async Task CreateCheep(CheepDTO Cheep)
    {
        await _cheepRepository.CreateCheep(Cheep);
    }
    public async Task DeleteUserCheeps(AuthorDTO Author)
    {
        await _cheepRepository.DeleteUserCheeps(Author);
    }
    
    public async Task DeleteCheep(int cheepId)
    {
        await _cheepRepository.DeleteCheep(cheepId);
    }

    public async Task DeleteComment(int commentId)
    {
        await _cheepRepository.DeleteComment(commentId);
    }
    
    public async Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheepsFromAnAuthor(string authorName)
    {
        return await _cheepRepository.RetrieveAllCheepsFromAnAuthor(authorName);
    }

    public async Task HandleLike(string authorName, int cheepId, string? emoji)
    {
        await _cheepRepository.HandleLike(authorName, cheepId, emoji);
    }

    public async Task HandleDislike(string authorName, int cheepId, string? emoji)
    {
        await _cheepRepository.HandleDislike(authorName, cheepId, emoji);

    }
    
    public async Task<List<CheepDTO>> GetPopularCheeps(int page)
    {
        return await _cheepRepository.GetPopularCheeps(page);
    }
    public async Task<int> GetTotalPageNumberForPopular()
    {
        return await _cheepRepository.GetTotalPageNumberForPopular();
    }

    public async Task<string> HandleImageUpload(IFormFile image)
    {
        return await _cheepRepository.HandleImageUpload(image);
    }
    public async Task<List<CommentDTO>> GetCommentsByCheepId(int cheepId)
    {
        return await _cheepRepository.GetCommentsByCheepId(cheepId);
    }
    public async Task AddCommentToCheep(CheepDTO cheepDto, string Text, string author)
    {
        await _cheepRepository.AddCommentToCheep(cheepDto, Text, author);
    }

    public async Task<CheepDTO?> GetCheepFromId(int cheepId)
    {
        return await _cheepRepository.GetCheepFromId(cheepId);
    }
    
    public async Task<List<String>> GetTopReactions (int cheepId)
    {
        return await _cheepRepository.GetTopReactions(cheepId);
    }
    
    public async Task<List<Core.DTOs.CommentDTO>> RetrieveAllCommentsFromAnAuthor(string authorName)
    {
        return await _cheepRepository.RetriveAllCommentsFromAnAuthor(authorName);
    }
    
}