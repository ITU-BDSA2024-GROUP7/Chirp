using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public class CheepDBContext : DbContext
{
    internal DbSet<Cheep> Cheeps { get; set; }
    DbSet<Author> Authors { get; set; }
    
    public CheepDBContext(DbContextOptions<CheepDBContext> options) : base(options) { }
}