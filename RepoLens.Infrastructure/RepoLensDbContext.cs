using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;

namespace RepoLens.Infrastructure;

public class RepoLensDbContext(DbContextOptions<RepoLensDbContext> options) : DbContext(options)
{
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<Commit> Commits => Set<Commit>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();
    public DbSet<ArtifactVersion> ArtifactVersions => Set<ArtifactVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Repository>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Url).IsUnique();
            entity.Property(r => r.Name).IsRequired();
            entity.Property(r => r.Url).IsRequired();
            entity.Property(r => r.LastSyncCommit).IsRequired();
        });

        modelBuilder.Entity<Commit>(entity =>
        {
            entity.HasKey(c => c.Sha);
            entity.HasIndex(c => new { c.RepositoryId, c.Sha }).IsUnique();
            entity.Property(c => c.Author).IsRequired();
            entity.Property(c => c.Timestamp).IsRequired();
            entity.Property(c => c.Message).IsRequired();
        });

        modelBuilder.Entity<Artifact>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => new { a.RepositoryId, a.Path }).IsUnique();
            entity.Property(a => a.Path).IsRequired();
        });

        modelBuilder.Entity<ArtifactVersion>(entity =>
        {
            entity.HasKey(av => av.Id);
            entity.HasIndex(av => new { av.ArtifactId, av.ContentHash }).IsUnique();
            entity.Property(av => av.ContentHash).IsRequired();
            entity.Property(av => av.StoredAt).IsRequired();
            entity.Property(av => av.Metadata).IsRequired();
        });
    }
}