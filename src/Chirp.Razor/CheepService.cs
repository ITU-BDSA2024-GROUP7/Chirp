/*
using Chirp.Razor;
public record CheepViewModel(string Author, string Message, string Timestamp); //7 fix record

public interface ICheepService
{
    public List<CheepViewModel> GetCheeps(int page);
    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page);
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

    public List<CheepViewModel> GetCheepsFromAuthor(string author, int page)
    {
       return _dbFacade.RetriveCheepFromAuthor(author, page);
    }

}
*/