namespace Chirp.Core.DTOs;

public class CheepDTO
{
    public int CheepId { get; set; }
    public required AuthorDTO Author { get; set; } // AuthorDTO
    public required string Text { get; set; } // Message text
    public string ImageReference { get; set; } // Message text
    public required string FormattedTimeStamp { get; set; } // Time stamp as a formatted string
    public ICollection<CommentDTO> Comments { get; set; } = new List<CommentDTO>(); // List of comments
    
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    
}