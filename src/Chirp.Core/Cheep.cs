using System.ComponentModel.DataAnnotations;


namespace Chirp.Core;

public class Cheep
{
    [Key]
    public int CheepId { get; set; } // Primary Key
    public required Author Author { get; set; } // Navigation Property
    public int AuthorId { get; set; }
    
    
    // Making sure that the total length of the message is less than 160 characters
    [Required]
    [StringLength(160)]
    public required string Text { get; set; }
    public string? ImageReference { get; set; }
    public DateTime TimeStamp { get; set; } 
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>(); // Navigation Property
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Dislike> Dislikes { get; set; } = new List<Dislike>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>(); 
}