using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

public class Comment
{
    [Key]
    public int CommentId { get; set; } // Primary Key
    public required int CheepId { get; set; } // Foreign key
    public required Author Author { get; set; } // Navigation Property
    
    [Required]
    [StringLength(160)]
    public required string Text { get; set; } // Message text
    public DateTime TimeStamp { get; set; } // Time stamp
}