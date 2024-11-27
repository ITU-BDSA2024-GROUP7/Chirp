using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Core;

public class Comment
{
    [Key]
    public int CommentId { get; set; } // Primary Key
    [ForeignKey("Cheep")]
    public int CheepId { get; set; } // Foreign key to Cheep
    public Cheep Cheep { get; set; } // Navigation property for Cheep
    
    [ForeignKey("Author")]
    public int AuthorId { get; set; } // Foreign key to Author
    public Author Author { get; set; } // Navigation property for Author
    
    [Required]
    [StringLength(160)]
    public required string Text { get; set; } // Message text
    public DateTime TimeStamp { get; set; } // Time stamp
}