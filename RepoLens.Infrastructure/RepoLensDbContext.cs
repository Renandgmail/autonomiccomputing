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

    // AST Analysis entities (Phase 2)
    public DbSet<ASTRepositoryAnalysis> ASTRepositoryAnalyses => Set<ASTRepositoryAnalysis>();
    public DbSet<ASTFileAnalysis> ASTFileAnalyses => Set<ASTFileAnalysis>();
    public DbSet<ASTStatement> ASTStatements => Set<ASTStatement>();
    public DbSet<ASTClass> ASTClasses => Set<ASTClass>();
    public DbSet<ASTMethod> ASTMethods => Set<ASTMethod>();
    public DbSet<ASTProperty> ASTProperties => Set<ASTProperty>();
    public DbSet<ASTParameter> ASTParameters => Set<ASTParameter>();
    public DbSet<ASTImport> ASTImports => Set<ASTImport>();
    public DbSet<ASTExport> ASTExports => Set<ASTExport>();
    public DbSet<ASTIssue> ASTIssues => Set<ASTIssue>();
    public DbSet<DuplicateCodeBlock> DuplicateCodeBlocks => Set<DuplicateCodeBlock>();
    public DbSet<ASTDependency> ASTDependencies => Set<ASTDependency>();
    public DbSet<ASTMetrics> ASTMetrics => Set<ASTMetrics>();
    public DbSet<ASTFileMetrics> ASTFileMetrics => Set<ASTFileMetrics>();

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

        // Configure AST Analysis entities (Phase 2)
        modelBuilder.Entity<ASTRepositoryAnalysis>(entity =>
        {
            entity.HasKey(ara => ara.Id);
            entity.HasIndex(ara => ara.RepositoryId).IsUnique();
            entity.HasIndex(ara => ara.AnalyzedAt);
            entity.Property(ara => ara.Version).HasMaxLength(50);
            
            entity.HasOne(ara => ara.Repository)
                  .WithMany()
                  .HasForeignKey(ara => ara.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTFileAnalysis>(entity =>
        {
            entity.HasKey(afa => afa.Id);
            entity.HasIndex(afa => afa.RepositoryAnalysisId);
            entity.HasIndex(afa => afa.FilePath);
            entity.HasIndex(afa => afa.Language);
            entity.HasIndex(afa => afa.IsSupported);
            
            entity.Property(afa => afa.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(afa => afa.Language).HasMaxLength(50);
            
            entity.HasOne(afa => afa.RepositoryAnalysis)
                  .WithMany(ara => ara.Files)
                  .HasForeignKey(afa => afa.RepositoryAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTStatement>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.FileAnalysisId);
            entity.HasIndex(s => s.Type);
            entity.HasIndex(s => s.Line);
            
            entity.Property(s => s.StatementId).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Type).IsRequired().HasMaxLength(100);
            entity.Property(s => s.CodeSnippet).HasMaxLength(2000);
            
            // Configure JSON column
            entity.Property(s => s.Dependencies)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");
                  
            entity.HasOne(s => s.FileAnalysis)
                  .WithMany(afa => afa.Statements)
                  .HasForeignKey(s => s.FileAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTClass>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.FileAnalysisId);
            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => c.IsAbstract);
            entity.HasIndex(c => c.IsInterface);
            
            entity.Property(c => c.Name).IsRequired().HasMaxLength(255);
            entity.Property(c => c.FullName).HasMaxLength(500);
            entity.Property(c => c.AccessModifier).HasMaxLength(50);
            entity.Property(c => c.BaseClass).HasMaxLength(255);
            
            // Configure JSON columns
            entity.Property(c => c.Interfaces)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");
                  
            entity.HasOne(c => c.FileAnalysis)
                  .WithMany(afa => afa.Classes)
                  .HasForeignKey(c => c.FileAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTMethod>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.FileAnalysisId);
            entity.HasIndex(m => m.ClassId);
            entity.HasIndex(m => m.Name);
            entity.HasIndex(m => m.IsStatic);
            entity.HasIndex(m => m.IsAsync);
            entity.HasIndex(m => m.CyclomaticComplexity);
            
            entity.Property(m => m.Name).IsRequired().HasMaxLength(255);
            entity.Property(m => m.Signature).HasMaxLength(1000);
            entity.Property(m => m.AccessModifier).HasMaxLength(50);
            entity.Property(m => m.ReturnType).HasMaxLength(255);
            
            // Configure JSON column
            entity.Property(m => m.CalledMethods)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");
                  
            entity.HasOne(m => m.FileAnalysis)
                  .WithMany(afa => afa.Methods)
                  .HasForeignKey(m => m.FileAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(m => m.Class)
                  .WithMany(c => c.Methods)
                  .HasForeignKey(m => m.ClassId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ASTProperty>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.ClassId);
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.IsStatic);
            
            entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
            entity.Property(p => p.Type).IsRequired().HasMaxLength(255);
            entity.Property(p => p.AccessModifier).HasMaxLength(50);
            
            entity.HasOne(p => p.Class)
                  .WithMany(c => c.Properties)
                  .HasForeignKey(p => p.ClassId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTParameter>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.MethodId);
            entity.HasIndex(p => p.Position);
            
            entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
            entity.Property(p => p.Type).IsRequired().HasMaxLength(255);
            entity.Property(p => p.DefaultValue).HasMaxLength(500);
            
            entity.HasOne(p => p.Method)
                  .WithMany(m => m.Parameters)
                  .HasForeignKey(p => p.MethodId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTImport>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.FileAnalysisId);
            entity.HasIndex(i => i.Module);
            entity.HasIndex(i => i.IsDefaultImport);
            entity.HasIndex(i => i.IsNamespaceImport);
            
            entity.Property(i => i.Module).IsRequired().HasMaxLength(500);
            
            // Configure JSON column
            entity.Property(i => i.ImportedSymbols)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                  .HasColumnType("TEXT");
                  
            entity.HasOne(i => i.FileAnalysis)
                  .WithMany(afa => afa.Imports)
                  .HasForeignKey(i => i.FileAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTExport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileAnalysisId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsDefault);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.FileAnalysis)
                  .WithMany(afa => afa.Exports)
                  .HasForeignKey(e => e.FileAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTIssue>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.FileAnalysisId);
            entity.HasIndex(i => i.MethodId);
            entity.HasIndex(i => i.Severity);
            entity.HasIndex(i => i.Category);
            entity.HasIndex(i => i.RuleId);
            
            entity.Property(i => i.Severity).IsRequired().HasMaxLength(50);
            entity.Property(i => i.IssueType).IsRequired().HasMaxLength(255);
            entity.Property(i => i.Category).IsRequired().HasMaxLength(100);
            entity.Property(i => i.Description).IsRequired().HasMaxLength(2000);
            entity.Property(i => i.Recommendation).HasMaxLength(2000);
            entity.Property(i => i.RuleId).HasMaxLength(100);
            entity.Property(i => i.MoreInfoUrl).HasMaxLength(500);
            
            entity.HasOne(i => i.FileAnalysis)
                  .WithMany(afa => afa.Issues)
                  .HasForeignKey(i => i.FileAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(i => i.Method)
                  .WithMany(m => m.Issues)
                  .HasForeignKey(i => i.MethodId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DuplicateCodeBlock>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.RepositoryAnalysisId);
            entity.HasIndex(d => d.GroupId);
            entity.HasIndex(d => d.SimilarityScore);
            entity.HasIndex(d => d.DuplicateType);
            
            entity.Property(d => d.GroupId).IsRequired().HasMaxLength(100);
            entity.Property(d => d.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(d => d.Hash).HasMaxLength(100);
            entity.Property(d => d.DuplicateType).HasMaxLength(100);
            entity.Property(d => d.CodeBlock).HasMaxLength(10000);
            
            entity.HasOne(d => d.RepositoryAnalysis)
                  .WithMany(ara => ara.DuplicateBlocks)
                  .HasForeignKey(d => d.RepositoryAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTDependency>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.RepositoryAnalysisId);
            entity.HasIndex(d => d.SourceFile);
            entity.HasIndex(d => d.TargetFile);
            entity.HasIndex(d => d.DependencyType);
            entity.HasIndex(d => d.IsCircular);
            
            entity.Property(d => d.SourceFile).IsRequired().HasMaxLength(1000);
            entity.Property(d => d.TargetFile).IsRequired().HasMaxLength(1000);
            entity.Property(d => d.DependencyType).IsRequired().HasMaxLength(100);
            
            entity.HasOne(d => d.RepositoryAnalysis)
                  .WithMany(ara => ara.Dependencies)
                  .HasForeignKey(d => d.RepositoryAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTMetrics>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.RepositoryAnalysisId).IsUnique();
            
            entity.HasOne(m => m.RepositoryAnalysis)
                  .WithOne(ara => ara.Metrics)
                  .HasForeignKey<ASTMetrics>(m => m.RepositoryAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ASTFileMetrics>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.FileAnalysisId).IsUnique();
            entity.Property(m => m.QualityTrend).HasMaxLength(50);
            
            entity.HasOne(m => m.FileAnalysis)
                  .WithOne(afa => afa.Metrics)
                  .HasForeignKey<ASTFileMetrics>(m => m.FileAnalysisId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
