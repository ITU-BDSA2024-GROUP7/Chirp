using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Core;

public class Comment
{
    [Key]
    public int CommentId { get; set; } // Primary Key
    
    [ForeignKey("CheepId")]
    public Cheep Cheep { get; set; } // Navigation property for Cheep
    public int CheepId { get; set; } // Foreign key to Cheep
    
    [ForeignKey("AuthorId")]
    public Author Author { get; set; } // Navigation property for Author
    public int AuthorId { get; set; } // Foreign key to Author
    
    [Required]
    [StringLength(160)]
    public required string Text { get; set; } // Message text
    public DateTime TimeStamp { get; set; } // Time stamp
}