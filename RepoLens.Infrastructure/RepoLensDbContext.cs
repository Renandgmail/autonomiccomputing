using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;

namespace RepoLens.Infrastructure;

public class RepoLensDbContext(DbContextOptions<RepoLensDbContext> options) 
    : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, 
        IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    // Repository entities
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<Commit> Commits => Set<Commit>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();
    public DbSet<ArtifactVersion> ArtifactVersions => Set<ArtifactVersion>();
    
    // Authentication entities
    public DbSet<Organization> Organizations => Set<Organization>();
    
    // Metrics entities
    public DbSet<RepositoryMetrics> RepositoryMetrics => Set<RepositoryMetrics>();
    public DbSet<ContributorMetrics> ContributorMetrics => Set<ContributorMetrics>();
    public DbSet<FileMetrics> FileMetrics => Set<FileMetrics>();
    
    // Code Intelligence entities (Action Item #3)
    public DbSet<RepositoryFile> RepositoryFiles => Set<RepositoryFile>();
    public DbSet<CodeElement> CodeElements => Set<CodeElement>();
    
    // Vocabulary entities (Action Item #5)
    public DbSet<VocabularyTerm> VocabularyTerms => Set<VocabularyTerm>();
    public DbSet<VocabularyLocation> VocabularyLocations => Set<VocabularyLocation>();
    public DbSet<VocabularyTermRelationship> VocabularyTermRelationships => Set<VocabularyTermRelationship>();
    public DbSet<BusinessConcept> BusinessConcepts => Set<BusinessConcept>();
    public DbSet<VocabularyStats> VocabularyStats => Set<VocabularyStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base configuration for Identity
        base.OnModelCreating(modelBuilder);

        // Configure authentication entities
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.TimeZone).HasMaxLength(50);
            entity.Property(u => u.ProfileImageUrl).HasMaxLength(500);
            
            // Configure JSON columns for SQLite using text storage
            entity.Property(u => u.Preferences)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<UserPreferences>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new UserPreferences())
                  .HasColumnType("TEXT");
            
            // Configure relationships
            entity.HasOne(u => u.Organization)
                  .WithMany(o => o.Users)
                  .HasForeignKey(u => u.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(256);
            entity.Property(r => r.Description).HasMaxLength(500);
            
            // Configure JSON columns for SQLite using text storage
            entity.Property(r => r.Permissions)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            
            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(ur => ur.AssignedByUser)
                  .WithMany()
                  .HasForeignKey(ur => ur.AssignedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Name).IsRequired().HasMaxLength(200);
            entity.Property(o => o.Description).HasMaxLength(1000);
            entity.Property(o => o.Website).HasMaxLength(500);
            
            // Configure JSON columns for SQLite using text storage
            entity.Property(o => o.Settings)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<OrganizationSettings>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new OrganizationSettings())
                  .HasColumnType("TEXT");
        });

        modelBuilder.Entity<Repository>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Url).IsUnique();
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Url).IsRequired().HasMaxLength(1000);
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.DefaultBranch).HasMaxLength(100);
            entity.Property(r => r.AccessToken).HasMaxLength(500);
            entity.Property(r => r.Username).HasMaxLength(100);
            entity.Property(r => r.SyncErrorMessage).HasMaxLength(2000);
            
            // Configure relationships
            entity.HasOne(r => r.Owner)
                  .WithMany(u => u.OwnedRepositories)
                  .HasForeignKey(r => r.OwnerId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(r => r.Organization)
                  .WithMany(o => o.Repositories)
                  .HasForeignKey(r => r.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            // Configure navigation properties
            entity.HasMany(r => r.Metrics)
                  .WithOne(rm => rm.Repository)
                  .HasForeignKey(rm => rm.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(r => r.ContributorMetrics)
                  .WithOne(cm => cm.Repository)
                  .HasForeignKey(cm => cm.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasMany(r => r.FileMetrics)
                  .WithOne(fm => fm.Repository)
                  .HasForeignKey(fm => fm.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
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

        // Configure RepositoryMetrics
        modelBuilder.Entity<RepositoryMetrics>(entity =>
        {
            entity.HasKey(rm => rm.Id);
            entity.HasIndex(rm => new { rm.RepositoryId, rm.MeasurementDate }).IsUnique();
            entity.Property(rm => rm.RepositoryId).IsRequired();
            entity.Property(rm => rm.MeasurementDate).IsRequired();
            
            // Configure JSON columns for SQLite using text storage
            entity.Property(rm => rm.LanguageDistribution).HasColumnType("TEXT");
            entity.Property(rm => rm.FileTypeDistribution).HasColumnType("TEXT");
            entity.Property(rm => rm.HourlyActivityPattern).HasColumnType("TEXT");
            entity.Property(rm => rm.DailyActivityPattern).HasColumnType("TEXT");
            
            // Ignore navigation property to avoid conflicts
            entity.Ignore(rm => rm.Repository);
        });

        // Configure ContributorMetrics
        modelBuilder.Entity<ContributorMetrics>(entity =>
        {
            entity.HasKey(cm => cm.Id);
            entity.HasIndex(cm => new { cm.RepositoryId, cm.ContributorEmail, cm.PeriodStart }).IsUnique();
            entity.Property(cm => cm.RepositoryId).IsRequired();
            entity.Property(cm => cm.ContributorName).IsRequired().HasMaxLength(255);
            entity.Property(cm => cm.ContributorEmail).IsRequired().HasMaxLength(255);
            entity.Property(cm => cm.PeriodStart).IsRequired();
            entity.Property(cm => cm.PeriodEnd).IsRequired();
            
            // Configure JSON columns for SQLite using text storage
            entity.Property(cm => cm.HourlyActivityPattern).HasColumnType("TEXT");
            entity.Property(cm => cm.WeeklyActivityPattern).HasColumnType("TEXT");
            entity.Property(cm => cm.MonthlyActivityPattern).HasColumnType("TEXT");
            entity.Property(cm => cm.LanguageContributions).HasColumnType("TEXT");
            entity.Property(cm => cm.FileTypeContributions).HasColumnType("TEXT");
            
            // Ignore navigation property to avoid conflicts
            entity.Ignore(cm => cm.Repository);
        });

        // Configure FileMetrics
        modelBuilder.Entity<FileMetrics>(entity =>
        {
            entity.HasKey(fm => fm.Id);
            entity.HasIndex(fm => new { fm.RepositoryId, fm.FilePath }).IsUnique();
            entity.Property(fm => fm.RepositoryId).IsRequired();
            entity.Property(fm => fm.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(fm => fm.FileName).IsRequired().HasMaxLength(255);
            entity.Property(fm => fm.FileExtension).HasMaxLength(50);
            entity.Property(fm => fm.PrimaryLanguage).HasMaxLength(100);
            entity.Property(fm => fm.FileCategory).HasMaxLength(100);
            entity.Property(fm => fm.LastAnalyzed).IsRequired();
            
            // Configure JSON columns for SQLite using text storage
            entity.Property(fm => fm.ContributorBreakdown).HasColumnType("TEXT");
            entity.Property(fm => fm.ChangePatterns).HasColumnType("TEXT");
            entity.Property(fm => fm.DependencyGraph).HasColumnType("TEXT");
            entity.Property(fm => fm.IssueHistory).HasColumnType("TEXT");
            
            // Ignore navigation property to avoid conflicts
            entity.Ignore(fm => fm.Repository);
        });

        // Configure Code Intelligence entities (Action Item #3)
        modelBuilder.Entity<RepositoryFile>(entity =>
        {
            entity.HasKey(rf => rf.Id);
            entity.HasIndex(rf => new { rf.RepositoryId, rf.FilePath }).IsUnique();
            entity.HasIndex(rf => rf.FileExtension);
            entity.HasIndex(rf => rf.Language);
            entity.HasIndex(rf => rf.ProcessingStatus);
            
            entity.Property(rf => rf.ProcessingStatus).HasConversion<string>();
            
            entity.HasOne(rf => rf.Repository)
                  .WithMany(r => r.RepositoryFiles)
                  .HasForeignKey(rf => rf.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CodeElement>(entity =>
        {
            entity.HasKey(ce => ce.Id);
            entity.HasIndex(ce => ce.FileId);
            entity.HasIndex(ce => ce.ElementType);
            entity.HasIndex(ce => ce.Name);
            
            entity.Property(ce => ce.ElementType).HasConversion<string>();
            entity.Property(ce => ce.Parameters).HasColumnType("TEXT"); // PostgreSQL JSON as TEXT
            
            entity.HasOne(ce => ce.RepositoryFile)
                  .WithMany(rf => rf.CodeElements)
                  .HasForeignKey(ce => ce.FileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Vocabulary entities (Action Item #5)
        modelBuilder.Entity<VocabularyTerm>(entity =>
        {
            entity.HasKey(vt => vt.Id);
            entity.HasIndex(vt => new { vt.RepositoryId, vt.NormalizedTerm }).IsUnique();
            entity.HasIndex(vt => vt.TermType);
            entity.HasIndex(vt => vt.Source);
            entity.HasIndex(vt => vt.Domain);
            entity.HasIndex(vt => vt.RelevanceScore);

            entity.Property(vt => vt.Term).IsRequired().HasMaxLength(200);
            entity.Property(vt => vt.NormalizedTerm).IsRequired().HasMaxLength(200);
            entity.Property(vt => vt.TermType).HasConversion<string>();
            entity.Property(vt => vt.Source).HasConversion<string>();
            entity.Property(vt => vt.Language).HasMaxLength(50);
            entity.Property(vt => vt.Context).HasMaxLength(2000);
            entity.Property(vt => vt.Definition).HasMaxLength(1000);
            entity.Property(vt => vt.Domain).HasMaxLength(100);

            // Configure JSON columns with proper serialization
            entity.Property(vt => vt.Synonyms)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(vt => vt.RelatedTerms)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(vt => vt.UsageExamples)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(vt => vt.Metadata)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                  .HasColumnType("TEXT");

            entity.HasOne(vt => vt.Repository)
                  .WithMany()
                  .HasForeignKey(vt => vt.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VocabularyLocation>(entity =>
        {
            entity.HasKey(vl => vl.Id);
            entity.HasIndex(vl => vl.VocabularyTermId);
            entity.HasIndex(vl => vl.FilePath);

            entity.Property(vl => vl.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(vl => vl.ContextType).HasConversion<string>();
            entity.Property(vl => vl.ContextDescription).HasMaxLength(500);
            entity.Property(vl => vl.SurroundingCode).HasMaxLength(2000);

            entity.HasOne(vl => vl.VocabularyTerm)
                  .WithMany(vt => vt.Locations)
                  .HasForeignKey(vl => vl.VocabularyTermId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VocabularyTermRelationship>(entity =>
        {
            entity.HasKey(vtr => vtr.Id);
            entity.HasIndex(vtr => new { vtr.FromTermId, vtr.ToTermId }).IsUnique();
            entity.HasIndex(vtr => vtr.RelationshipType);

            entity.Property(vtr => vtr.RelationshipType).HasConversion<string>();
            entity.Property(vtr => vtr.Context).HasMaxLength(500);
            entity.Property(vtr => vtr.Evidence)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.HasOne(vtr => vtr.FromTerm)
                  .WithMany(vt => vt.FromRelationships)
                  .HasForeignKey(vtr => vtr.FromTermId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(vtr => vtr.ToTerm)
                  .WithMany(vt => vt.ToRelationships)
                  .HasForeignKey(vtr => vtr.ToTermId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BusinessConcept>(entity =>
        {
            entity.HasKey(bc => bc.Id);
            entity.HasIndex(bc => bc.RepositoryId);
            entity.HasIndex(bc => bc.Domain);
            entity.HasIndex(bc => bc.ConceptType);

            entity.Property(bc => bc.Name).IsRequired().HasMaxLength(200);
            entity.Property(bc => bc.Description).HasMaxLength(1000);
            entity.Property(bc => bc.Domain).HasMaxLength(100);
            entity.Property(bc => bc.ConceptType).HasConversion<string>();

            // Configure JSON columns
            entity.Property(bc => bc.Keywords)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(bc => bc.TechnicalMappings)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(bc => bc.BusinessPurposes)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(bc => bc.RelatedTermIds)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<int>())
                  .HasColumnType("TEXT");

            entity.Property(bc => bc.Properties)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                  .HasColumnType("TEXT");

            entity.HasOne(bc => bc.Repository)
                  .WithMany()
                  .HasForeignKey(bc => bc.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VocabularyStats>(entity =>
        {
            entity.HasKey(vs => vs.Id);
            entity.HasIndex(vs => vs.RepositoryId).IsUnique();

            // Configure JSON columns with proper serialization
            entity.Property(vs => vs.LanguageDistribution)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, int>())
                  .HasColumnType("TEXT");

            entity.Property(vs => vs.DomainDistribution)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, int>())
                  .HasColumnType("TEXT");

            entity.Property(vs => vs.SourceDistribution)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<VocabularySource, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<VocabularySource, int>())
                  .HasColumnType("TEXT");

            entity.Property(vs => vs.TopDomains)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(vs => vs.EmergingTerms)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.Property(vs => vs.DeprecatedTerms)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");

            entity.HasOne(vs => vs.Repository)
                  .WithMany()
                  .HasForeignKey(vs => vs.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
