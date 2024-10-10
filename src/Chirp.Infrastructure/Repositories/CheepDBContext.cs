using Chirp.Core;
using Chirp.Razor;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepDBContext : DbContext
{
    public  DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }
    
    public CheepDBContext(DbContextOptions<CheepDBContext> options) : base(options) { }
}