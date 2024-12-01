using Microsoft.AspNetCore.Http;

namespace Chirp.Core.DTOs;

public class CheepDTO
{
    public int CheepId { get; set; }
    public required AuthorDTO Author { get; set; } // AuthorDTO
    public required string Text { get; set; } // Message text
    public string ImageReference { get; set; } // Message text
    public required String FormattedTimeStamp { get; set; } // Time stamp as a formatted string
    public ICollection<CommentDTO> Comments { get; set; } = new List<CommentDTO>(); // List of comments
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Dislike> Dislikes { get; set; } = new List<Dislike>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>(); // List of reactions
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public int CommentsCount { get; set; }
}