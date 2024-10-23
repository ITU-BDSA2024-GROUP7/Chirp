using Chirp.Core;
using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepDBContext : IdentityDbContext<ApplicationUser>
{
    public CheepDBContext(DbContextOptions<CheepDBContext> options) : base(options) { }
    
    public  DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }
}