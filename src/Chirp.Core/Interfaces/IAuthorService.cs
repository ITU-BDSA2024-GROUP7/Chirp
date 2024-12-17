using Chirp.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace Chirp.Core.Interfaces;

public interface IAuthorService
{
    public Task<AuthorDTO?> FindAuthorByName(String name);
    public Task CreateAuthor(string authorName, string authorEmail, string profilePicture = null);
    public Task DeleteUser(AuthorDTO Author);
    public Task FollowAuthor(string userAuthorName, string followedAuthorName);
    public Task UnfollowAuthor(string userAuthor, string authorToBeRemoved);
    public Task RemovedAuthorFromFollowingList(string authorName);
    public Task <List<string>>GetFollowedAuthors(string userName);
    public Task<List<string>> GetFollowingAuthors(string userName);
    public Task<int> GetKarmaForAuthor(string authorName);
    public Task UpdateProfilePicture(string authorName, IFormFile profilePicture);
    public Task ClearProfilePicture(string authorName, IFormFile profilePicture);
}