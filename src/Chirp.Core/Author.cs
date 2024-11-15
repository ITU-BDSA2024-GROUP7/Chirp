using System.ComponentModel.DataAnnotations;


namespace Chirp.Core;

public class Author
{
  [Key]
  public int AuthorId { get; set; } // Primary key
  
  public required string Name { get; set; } // Name of the author
  public required string Email { get; set; } // Email of the author
  public required ICollection<Cheep> Cheeps { get; set; } // List of all cheeps from the author
}