
using Chirp.Razor;
public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps(int page);
    public List<CheepViewModel> GetCheepsFromAuthor(string author);
}

public class CheepService : ICheepService
{
    private readonly DBFacade _dbFacade;

    public CheepService(DBFacade dbFacade)
    {
        _dbFacade = dbFacade;
        Console.WriteLine($"CheepService constructor called with DBFacade");
    }
    
    
    public List<CheepViewModel> GetCheeps(int page)
    {
        return _dbFacade.RetriveAllCheeps(page);
    }

    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
       return _dbFacade.RetriveCheepFromAuthor(author);
    }

}
