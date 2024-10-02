using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public class CheepDBContext : DbContext
{
    public  DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }
    
    public CheepDBContext(DbContextOptions<CheepDBContext> options) : base(options) { }
    
}