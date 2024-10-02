namespace Chirp.Razor;

public class Cheep
{
    public int CheepId { get; set; } // Primary Key
    public Author Author { get; set; } // Navigation Property
    public string Text { get; set; }
    public DateTime TimeStamp { get; set; } 
}