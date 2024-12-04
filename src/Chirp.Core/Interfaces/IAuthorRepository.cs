using Chirp.Core.DTOs;

namespace Chirp.Core.Interfaces;

public interface IAuthorRepository
{
    Task<AuthorDTO?> FindAuthorByNameDTO(String name);
    Task<Author?> FindAuthorByName(String name);
    Task CreateAuthor(string authorName, string authorEmail, string profilePicture);
    Task DeleteUser(AuthorDTO author);
    Task FollowAuthor(string userAuthor, string followedAuthor);
    Task UnfollowAuthor(string userAuthor, string authorToBeRemoved);
    Task RemovedAuthorFromFollowingList(string authorName);
    Task<List<string>> GetFollowedAuthors(string userName);
    Task<int> GetKarmaForAuthor(string authorName);
}