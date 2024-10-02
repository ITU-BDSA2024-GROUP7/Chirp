namespace Chirp.Razor;

public class Author
{
  public string AuthorId { get; set; } // Primary key
  public string Name { get; set; } // Name of the author
  public string Email { get; set; } // Email of the author
  public ICollection<Cheep> Cheeps { get; set; } // List of all cheeps from the author
}