using Chirp.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    public Task<List<Core.DTOs.CheepDTO>> GetCheeps(int page);
    public Task<List<Core.DTOs.CheepDTO>> GetCheepsFromAuthor(string author, int page);
    
    public Task<int> GetTotalPageNumber(string authorName);
    
    public Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheeps();
    
    public Task CreateCheep(CheepDTO Cheep);
    
    public Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheepsFromAnAuthor(string authorName);
   
    public Task DeleteUserCheeps(AuthorDTO Author);
    
    public Task HandleLike(string authorName, int cheepId, string? emoji);
    public Task HandleDislike(string authorName, int cheepId, string? emoji);
    public Task<List<CheepDTO>> GetPopularCheeps(int page);
    public Task<int> GetTotalPageNumberForPopular();
    public Task<string> HandleImageUpload(IFormFile image);
}