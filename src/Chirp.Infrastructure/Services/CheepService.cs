using Chirp.Core.DTOs;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Infrastructure.Services;
public interface ICheepService
{
    public Task<List<Core.DTOs.CheepDTO>> GetCheeps(int page);
    public Task<List<Core.DTOs.CheepDTO>> GetCheepsFromAuthor(string author, int page);
    
    public Task<int> GetTotalPageNumber(string authorName);
    
    public Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheeps();
    
    public Task CreateCheep(CheepDTO Cheep);
}
public class CheepService : ICheepService
{
    private readonly CheepRepository _cheepRepository;
    public CheepService(CheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
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
    
    public async Task<List<Core.DTOs.CheepDTO>> RetrieveAllCheeps()
    {
        return await _cheepRepository.RetrieveAllCheepsForEndPoint();
    }
    
    public async Task CreateCheep(CheepDTO Cheep)
    {
        await _cheepRepository.CreateCheep(Cheep);
    }
}