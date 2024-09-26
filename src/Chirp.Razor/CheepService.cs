public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    // The lists used to contain the Cheeps (either all Cheeps in general (GetCheeps) or Cheeps only from the author
    public List<CheepViewModel> GetCheeps();
    public List<CheepViewModel> GetCheepsFromAuthor(string author);
}

/**
 *
 * 
 */
public class CheepService : ICheepService
{
    // These would normally be loaded from a database for example
    private static readonly List<CheepViewModel> _cheeps = new()
        {
            new CheepViewModel("Helge", "Hello, BDSA students!", UnixTimeStampToDateTimeString(1690892208)),
            new CheepViewModel("Adrian", "Hej, velkommen til kurset.", UnixTimeStampToDateTimeString(1690895308)),
        };
    /// <summary>
    /// Gets all Cheeps from the given list
    /// </summary>
    /// <returns>Returns a list of Cheeps</returns>
    public List<CheepViewModel> GetCheeps()
    {
        return _cheeps;
    }
    
    /// <summary>
    /// Gets Cheeps on the given page
    /// </summary>
    /// <param name="page">number representing the page</param>
    /// <returns>32 Cheeps from the given page</returns>
    public List<CheepViewModel> GetPageCheeps(int page)
    {
        return _cheeps;
        // Query: SELECT * FROM cheeps ORDER BY unixtimestamp ASC OFFSET (page - 1) * 32
    }
    
    /// <summary>
    /// Gets Cheeps from the given author
    /// </summary>
    /// <param name="author">Author who wrote the cheep</param>
    /// <returns>A List of Cheeps only from the author</returns>
    public List<CheepViewModel> GetCheepsFromAuthor(string author)
    {
        // filter by the provided author name
        return _cheeps.Where(x => x.Author == author).ToList();
    }
    /// <summary>
    /// Converts Unix Timestamp to a DateTime string
    /// </summary>
    /// <param name="unixTimeStamp">The unix timestamp from the Cheep</param>
    /// <returns></returns>
    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }
}
