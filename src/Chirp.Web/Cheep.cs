using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor;

public class Cheep
{
    public int CheepId { get; set; } // Primary Key
    public required Author Author { get; set; } // Navigation Property
    public int AuthorId { get; set; }
    
    // Making sure that the total length of the message is less than 160 characters
    [Required]
    [StringLength(160)]
    public required string Text { get; set; }
    public DateTime TimeStamp { get; set; } 
}