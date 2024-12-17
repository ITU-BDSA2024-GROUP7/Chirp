using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Chirp.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    
    private readonly AuthorRepository _authorRepository;
    
    public AuthorService(AuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }
    
    public async Task<AuthorDTO?> FindAuthorByName(String name)
    {
        return await _authorRepository.FindAuthorByNameDTO(name);
    }
    
    public async Task CreateAuthor(string authorName, string authorEmail, string profilePicture = null)
    {
        await _authorRepository.CreateAuthor(authorName, authorEmail, profilePicture);
    }
    
    public async Task DeleteUser(AuthorDTO Author)
    {
        await _authorRepository.DeleteUser(Author);
    }
    
    public async Task FollowAuthor(string userAuthorName, string followedAuthorName)
    {
        await _authorRepository.FollowAuthor(userAuthorName, followedAuthorName);
    }
    
    public async Task UnfollowAuthor(string userAuthor, string authorToBeRemoved)
    {
        await _authorRepository.UnfollowAuthor(userAuthor, authorToBeRemoved);
    }
    
    public async Task RemovedAuthorFromFollowingList(string authorName)
    {
        await _authorRepository.RemovedAuthorFromFollowingList(authorName);
    }
    
    public async Task<List<string>> GetFollowedAuthors(string userName)
    {
        return await _authorRepository.GetFollowedAuthors(userName);
    }
    
    public async Task<List<string>> GetFollowingAuthors(string userName)
    {
        return await _authorRepository.GetFollowingAuthors(userName);
    }
    
    public async Task<int> GetKarmaForAuthor(string authorName)
    {
        return await _authorRepository.GetKarmaForAuthor(authorName);
    }
    
    public async Task UpdateProfilePicture(string authorName, IFormFile profilePicture)
    {
        await _authorRepository.UpdateProfilePicture(authorName, profilePicture);
    }
    
    public async Task ClearProfilePicture(string authorName, IFormFile profilePicture)
    {
        await _authorRepository.ClearProfilePicture(authorName, profilePicture);
    }
}