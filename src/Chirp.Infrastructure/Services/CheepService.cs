using Chirp.Core;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Chirp.Infrastructure.Services;
public interface ICheepService
{
    public Task<List<Core.DTOs.CheepDTO>> GetCheeps(int page);
    public Task<List<Core.DTOs.CheepDTO>> GetCheepsFromAuthor(string author, int page);
    
    public Task<int> GetTotalPageNumber(string authorName);
    
    public Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheeps();
    
    public Task CreateCheep(CheepDTO Cheep);
    
    public Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheepsFromAnAuthor(string authorName);
   
    public Task DeleteUserCheeps(AuthorDTO Author);
    
    public Task <List<string>>GetFollowedAuthors(string userName);
    public Task HandleLike(string authorName, int cheepId, string? emoji);
    public Task HandleDislike(string authorName, int cheepId, string? emoji);
    public Task<List<CheepDTO>> GetPopularCheeps(int page);
    public Task<int> GetTotalPageNumberForPopular();
    public Task<string> HandleImageUpload(IFormFile image);
}
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

    public async Task CreateAuthor(string authorName, string authorEmail)
    {
        await _authorRepository.CreateAuthor(authorName, authorEmail);
    }

    public async Task<AuthorDTO>? FindAuthorByName(String name)
    {
        return await _authorRepository.FindAuthorByNameDTO(name);
    }
    
    public async Task CreateCheep(CheepDTO Cheep)
    {
        await _cheepRepository.CreateCheep(Cheep);
    }
    public async Task DeleteUserCheeps(AuthorDTO Author)
    {
        await _cheepRepository.DeleteUserCheeps(Author);
    }

    public async Task DeleteUser(AuthorDTO Author)
    {
        await _authorRepository.DeleteUser(Author);
    }
    
    public async Task DeleteCheep(int cheepId)
    {
        await _cheepRepository.DeleteCheep(cheepId);
    }

    public async Task DeleteComment(int commentId)
    {
        await _cheepRepository.DeleteComment(commentId);
    }

    public async Task FollowAuthor(string userAuthorName, string followedAuthorName)
    {
        await _authorRepository.FollowAuthor(userAuthorName, followedAuthorName);
    }

    public async Task UnfollowAuthor(string userAuthor, string authorToBeRemoved)
    {
        await _authorRepository.UnfollowAuthor(userAuthor, authorToBeRemoved);
    }
    public async Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheepsFromAnAuthor(string authorName)
    {
        return await _cheepRepository.RetrieveAllCheepsFromAnAuthor(authorName);
    }
    
    public async Task RemovedAuthorFromFollowingList(string authorName)
    {
        await _authorRepository.RemovedAuthorFromFollowingList(authorName);
    }
    public async Task<List<string>> GetFollowedAuthors(string userName)
    {
        return await _authorRepository.GetFollowedAuthors(userName);
    }


    public async Task HandleLike(string authorName, int cheepId, string? emoji)
    {
        await _cheepRepository.HandleLike(authorName, cheepId, emoji);
    }

    public async Task HandleDislike(string authorName, int cheepId, string? emoji)
    {
        await _cheepRepository.HandleDislike(authorName, cheepId, emoji);

    }

    /// <summary>
    /// Returns the list of authors that follows a user
    /// </summary>
    /// <param name="userName">The username from the url</param>
    /// <returns></returns>
    public async Task<List<string>> GetFollowingAuthors(string userName)
    {
        return await _authorRepository.GetFollowingAuthors(userName);
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

    public async Task<CheepDTO> GetCheepFromId(int cheepId)
    {
        return await _cheepRepository.GetCheepFromId(cheepId);
    }
    public async Task<int> GetKarmaForAuthor(string authorName)
    {
        return await _authorRepository.GetKarmaForAuthor(authorName);
    }
    
    public async Task<List<String>> GetTopReactions (int cheepId)
    {
        return await _cheepRepository.GetTopReactions(cheepId);
    }
}