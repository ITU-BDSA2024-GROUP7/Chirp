using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Core;

public class Cheep
{
    [Key]
    public int CheepId { get; set; } // Primary Key
    public required Author Author { get; set; } // Navigation Property
    [ForeignKey("AuthorId")]
    public int AuthorId { get; set; }
    
    // Making sure that the total length of the message is less than 160 characters
    [Required]
    [StringLength(160)]
    public required string Text { get; set; }
    public DateTime TimeStamp { get; set; } 
}