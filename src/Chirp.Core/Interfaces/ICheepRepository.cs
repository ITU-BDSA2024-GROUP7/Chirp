using Chirp.Core.DTOs;

namespace Chirp.Core.Interfaces;

public interface ICheepRepository
{
    Task<List<CheepDTO>> ReadCheepsFromAuthor(string userName, int page);
    Task<List<CheepDTO>> ReadAllCheeps(int page);
    Task CreateCheep(CheepDTO newCheep);
    Task UpdateCheep(CheepDTO alteredCheep);
    Task<int> GetTotalPages(string authorName);
    Task DeleteCheep(int cheepId);
    Task DeleteUserData(AuthorDTO author);
    Task FollowAuthor(string userAuthor, string followedAuthor);
    Task UnfollowAuthor(string userAuthor, string authorToBeRemoved);
}