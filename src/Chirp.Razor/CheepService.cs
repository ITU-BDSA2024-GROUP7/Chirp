using Chirp.Razor;

namespace Chirp.Razor;
public interface ICheepService
{
    public Task<List<CheepDTO>> GetCheeps(int page);
    public Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int page);
}
public class CheepService : ICheepService
{
    private readonly CheepRepository _cheepRepository;
    public CheepService(CheepRepository cheepRepository)
    {
        _cheepRepository = cheepRepository;
    }
 
    
    
    public async Task<List<CheepDTO>> GetCheeps(int page)
    {
        return await _cheepRepository.ReadAllCheeps(page);
    }
    
    public async Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int page)
    {
        return await _cheepRepository.ReadCheepsFromAuthor(author, page);
    }
}

// Data Transfer Object for Cheep
public class CheepDTO
{
    public required string AuthorName { get; set; } // Author's name
    public required string Text { get; set; } // Message text
    public required string FormattedTimeStamp { get; set; } // Time stamp as a formatted string
}

// Data Transfer Object for Author
public class AuthorDTO
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}