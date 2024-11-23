using Chirp.Core.DTOs;

namespace Chirp.Core.Interfaces;

public interface IAuthorRepository
{
    Task CreateAuthor(string authorName, string authorEmail);
    Task DeleteUser(AuthorDTO Author);
    Task FollowAuthor(string userAuthor, string followedAuthor);
    Task UnfollowAuthor(string userAuthor, string authorToBeRemoved);
    Task RemovedAuthorFromFollowingList(string authorName);
}