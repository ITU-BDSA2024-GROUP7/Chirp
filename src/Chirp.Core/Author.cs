using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Chirp.Core.DTOs;
using Microsoft.VisualBasic.CompilerServices;


namespace Chirp.Core;

public class Author
{
  [Key]
  public int AuthorId { get; set; } // Primary key
  
  public required string Name { get; set; } // Name of the author
  public required string Email { get; set; } // Email of the author
  public required ICollection<Cheep> Cheeps { get; set; } // List of all cheeps from the author
  public required ICollection<string> AuthorsFollowed { get; set; } // List of authors followed
  public ICollection<Like> Likes { get; set; } = new List<Like>();
  public ICollection<Dislike> Dislikes { get; set; } = new List<Dislike>();
  public string? ProfilePicture { get; set; } // Profile picture of the author
}