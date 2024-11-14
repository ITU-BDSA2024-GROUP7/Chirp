using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic.CompilerServices;

namespace Chirp.Core;

public class Author
{
  [Key]
  public int AuthorId { get; set; } // Primary key
  
  public required string Name { get; set; } // Name of the author
  public required string Email { get; set; } // Email of the author
  public required ICollection<Cheep> Cheeps { get; set; } // List of all cheeps from the author
  public required ICollection<string> FollowedAuthors { get; set; } // List of all authors that this author follows
}