using Chirp.Core;
using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepDBContext(DbContextOptions<CheepDBContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public  DbSet<Cheep> Cheeps { get; set; }
    public DbSet<Author> Authors { get; set; }
    
    public DbSet<Comment> Comment { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Dislike> Dislikes { get; set; }
    public DbSet<Reaction> Reaction { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure IdentityDbContext configurations are applied
        base.OnModelCreating(modelBuilder);
        
        // Configure the many-to-many relationship between Cheeps and Authors through Likes
        modelBuilder.Entity<Like>()
            .HasOne(l => l.Cheep)
            .WithMany(c => c.Likes)
            .HasForeignKey(l => l.CheepId);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Author)
            .WithMany(a => a.Likes)
            .HasForeignKey(l => l.AuthorId);
        
        // Configure the many-to-many relationship between Cheeps and Authors through Dislikes
        modelBuilder.Entity<Dislike>()
            .HasOne(l => l.Cheep)
            .WithMany(c => c.Dislikes)
            .HasForeignKey(l => l.CheepId);

        modelBuilder.Entity<Dislike>()
            .HasOne(l => l.Author)
            .WithMany(a => a.Dislikes)
            .HasForeignKey(l => l.AuthorId);
        
        // Configure the one-to-many relationship between Cheeps and Authors
        modelBuilder.Entity<Cheep>()
            .HasOne(c => c.Author)
            .WithMany(a => a.Cheeps)
            .HasForeignKey(c => c.AuthorId);
        
        // Configure the one-to-many relationship between Comments and Authors
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Cheep)
            .WithMany(c => c.Comments)
            .HasForeignKey(c => c.CheepId);
        
        // Configure the many-to-many relationship between Cheeps and Authors through Reactions
        modelBuilder.Entity<Reaction>()
            .HasOne(r => r.Cheep)
            .WithMany(c => c.Reactions)
            .HasForeignKey(r => r.CheepId);
        
    }
}