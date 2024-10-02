namespace Chirp.Razor;

public class Cheep
{
    public int MessageId { get; set; } // Primary Key
    public Author AuthorId { get; set; } // Navigation Property
    public string Text { get; set; }
    public DateTime TimeStamp { get; set; } 
}