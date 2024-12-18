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
    /// <summary>
    /// Finds an author in the database using the name of the author
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<AuthorDTO?> FindAuthorByName(String name)
    {
        return await _authorRepository.FindAuthorByNameDTO(name);
    }
    // <summary>
    ///  This method is for creating a new author
    /// </summary>
    /// <param name="authorName"></param>
    /// <param name="authorEmail"></param>
    /// <param name="profilePicture"></param>
    public async Task CreateAuthor(string authorName, string authorEmail, string profilePicture = null)
    {
        await _authorRepository.CreateAuthor(authorName, authorEmail, profilePicture);
    }
    /// <summary>
    /// This method is used for deleting an author
    /// </summary>
    /// <param name="Author"></param>
    public async Task DeleteUser(AuthorDTO Author)
    {
        await _authorRepository.DeleteUser(Author);
    }
    /// <summary>
    /// This Method is for Following an author
    /// </summary>
    /// <param name="userAuthorName"></param>
    /// <param name="followedAuthorName"></param>
    public async Task FollowAuthor(string userAuthorName, string followedAuthorName)
    {
        await _authorRepository.FollowAuthor(userAuthorName, followedAuthorName);
    }
    /// <summary>
    /// Unfollows an author
    /// </summary>
    /// <param name="userAuthorName"></param>
    /// <param name="authorToBeRemoved"></param>
    public async Task UnfollowAuthor(string userAuthor, string authorToBeRemoved)
    {
        await _authorRepository.UnfollowAuthor(userAuthor, authorToBeRemoved);
    }
    /// <summary>
    /// When deleting user data we need to delete the username for every other author list. 
    /// </summary>
    /// <param name="authorName"></param>
    /// <param name="page"></param>
    public async Task RemovedAuthorFromFollowingList(string authorName)
    {
        await _authorRepository.RemovedAuthorFromFollowingList(authorName);
    }
    /// <summary>
    /// Returns the list of authors that a user follows
    /// </summary>
    /// <param name="userName">The username from the url</param>
    /// <returns></returns>
    public async Task<List<string>> GetFollowedAuthors(string userName)
    {
        return await _authorRepository.GetFollowedAuthors(userName);
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
    /// <summary>
    /// This methods returns the amount of karma a specific user has
    /// </summary>
    /// <param name="authorName"></param>
    /// <returns>Karma</returns>
    public async Task<int> GetKarmaForAuthor(string authorName)
    {
        return await _authorRepository.GetKarmaForAuthor(authorName);
    }
    /// <summary>
    /// Method for updating your profile picture
    /// </summary>
    /// <param name="authorName"></param>
    /// <param name="profilePicture"></param>
    public async Task UpdateProfilePicture(string authorName, IFormFile profilePicture)
    {
        await _authorRepository.UpdateProfilePicture(authorName, profilePicture);
    }
    /// <summary>
    /// Method for clearing a users profilepicture
    /// </summary>
    /// <param name="authorName"></param>
    /// <param name="profilePicture"></param>
    public async Task ClearProfilePicture(string authorName, IFormFile profilePicture)
    {
        await _authorRepository.ClearProfilePicture(authorName, profilePicture);
    }
}