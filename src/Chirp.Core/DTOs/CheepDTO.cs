namespace Chirp.Core.DTOs;

public class CheepDTO
{
    public AuthorDTO Author { get; set; } // AuthorDTO
    public required string Text { get; set; } // Message text
    public required string FormattedTimeStamp { get; set; } // Time stamp as a formatted string
}