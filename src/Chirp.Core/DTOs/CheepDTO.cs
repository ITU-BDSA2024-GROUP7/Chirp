namespace Chirp.Core.DTOs;

public class CheepDTO
{
    public required string AuthorName { get; set; } // Author's name
    public required string Text { get; set; } // Message text
    public required string FormattedTimeStamp { get; set; } // Time stamp as a formatted string
}