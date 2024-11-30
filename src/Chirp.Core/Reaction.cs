using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Core;

public class Reaction
{
    [Key]
    public int ReactionId { get; set; } // Primary key for the reaction entity
    
    [ForeignKey("Cheep")]
    public int CheepId { get; set; } // Foreign key to Cheep
    public Cheep Cheep { get; set; } // Navigation property for Cheep
    
    [ForeignKey("Author")]
    public int AuthorId { get; set; } // Foreign key to Author
    public Author Author { get; set; } // Navigation property for Author
    
    [Required]
    public string Emoji { get; set; } // The emoji representing the reaction
}