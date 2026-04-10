using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RepoLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArtifactVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtifactId = table.Column<int>(type: "integer", nullable: false),
                    CommitSha = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: false),
                    StoredAt = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    Permissions = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    Sha = table.Column<string>(type: "text", nullable: false),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.Sha);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Settings = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Preferences = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LastSyncCommit = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DefaultBranch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAnalysisAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutoSync = table.Column<bool>(type: "boolean", nullable: false),
                    SyncIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    SyncErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AccessToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Repositories_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Repositories_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Artifacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artifacts_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContributorMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    ContributorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContributorEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommitCount = table.Column<int>(type: "integer", nullable: false),
                    LinesAdded = table.Column<int>(type: "integer", nullable: false),
                    LinesDeleted = table.Column<int>(type: "integer", nullable: false),
                    LinesModified = table.Column<int>(type: "integer", nullable: false),
                    FilesModified = table.Column<int>(type: "integer", nullable: false),
                    FilesAdded = table.Column<int>(type: "integer", nullable: false),
                    FilesDeleted = table.Column<int>(type: "integer", nullable: false),
                    ContributionPercentage = table.Column<double>(type: "double precision", nullable: false),
                    WorkingDays = table.Column<int>(type: "integer", nullable: false),
                    AverageCommitSize = table.Column<double>(type: "double precision", nullable: false),
                    CommitFrequency = table.Column<double>(type: "double precision", nullable: false),
                    LongestCommitStreak = table.Column<int>(type: "integer", nullable: false),
                    CurrentCommitStreak = table.Column<int>(type: "integer", nullable: false),
                    OwnedFiles = table.Column<int>(type: "integer", nullable: false),
                    CodeOwnershipPercentage = table.Column<double>(type: "double precision", nullable: false),
                    UniqueFilesTouched = table.Column<int>(type: "integer", nullable: false),
                    PullRequestsCreated = table.Column<int>(type: "integer", nullable: false),
                    PullRequestsReviewed = table.Column<int>(type: "integer", nullable: false),
                    CodeReviewComments = table.Column<int>(type: "integer", nullable: false),
                    IssuesCreated = table.Column<int>(type: "integer", nullable: false),
                    IssuesResolved = table.Column<int>(type: "integer", nullable: false),
                    MentionedInCommits = table.Column<int>(type: "integer", nullable: false),
                    HourlyActivityPattern = table.Column<string>(type: "TEXT", nullable: true),
                    WeeklyActivityPattern = table.Column<string>(type: "TEXT", nullable: true),
                    MonthlyActivityPattern = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageContributions = table.Column<string>(type: "TEXT", nullable: true),
                    FileTypeContributions = table.Column<string>(type: "TEXT", nullable: true),
                    AvgCommitMessageLength = table.Column<double>(type: "double precision", nullable: false),
                    CommitMessageQualityScore = table.Column<double>(type: "double precision", nullable: false),
                    BugFixCommits = table.Column<int>(type: "integer", nullable: false),
                    FeatureCommits = table.Column<int>(type: "integer", nullable: false),
                    RefactoringCommits = table.Column<int>(type: "integer", nullable: false),
                    DocumentationCommits = table.Column<int>(type: "integer", nullable: false),
                    IsCoreContributor = table.Column<bool>(type: "boolean", nullable: false),
                    IsNewContributor = table.Column<bool>(type: "boolean", nullable: false),
                    FirstContribution = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastContribution = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysActive = table.Column<int>(type: "integer", nullable: false),
                    RetentionScore = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContributorMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContributorMetrics_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrimaryLanguage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastAnalyzed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    EffectiveLineCount = table.Column<int>(type: "integer", nullable: false),
                    CommentLineCount = table.Column<int>(type: "integer", nullable: false),
                    BlankLineCount = table.Column<int>(type: "integer", nullable: false),
                    CommentDensity = table.Column<double>(type: "double precision", nullable: false),
                    CyclomaticComplexity = table.Column<double>(type: "double precision", nullable: false),
                    CognitiveComplexity = table.Column<double>(type: "double precision", nullable: false),
                    MethodCount = table.Column<int>(type: "integer", nullable: false),
                    ClassCount = table.Column<int>(type: "integer", nullable: false),
                    FunctionCount = table.Column<int>(type: "integer", nullable: false),
                    AverageMethodLength = table.Column<double>(type: "double precision", nullable: false),
                    MaxMethodLength = table.Column<double>(type: "double precision", nullable: false),
                    NestingDepth = table.Column<double>(type: "double precision", nullable: false),
                    CodeSmellCount = table.Column<int>(type: "integer", nullable: false),
                    CriticalIssues = table.Column<int>(type: "integer", nullable: false),
                    MajorIssues = table.Column<int>(type: "integer", nullable: false),
                    MinorIssues = table.Column<int>(type: "integer", nullable: false),
                    MaintainabilityIndex = table.Column<double>(type: "double precision", nullable: false),
                    TechnicalDebtMinutes = table.Column<double>(type: "double precision", nullable: false),
                    DuplicationLines = table.Column<int>(type: "integer", nullable: false),
                    DuplicationPercentage = table.Column<double>(type: "double precision", nullable: false),
                    TotalCommits = table.Column<int>(type: "integer", nullable: false),
                    UniqueContributors = table.Column<int>(type: "integer", nullable: false),
                    FirstCommit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastCommit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommitsLastMonth = table.Column<int>(type: "integer", nullable: false),
                    CommitsLastQuarter = table.Column<int>(type: "integer", nullable: false),
                    ChangeFrequency = table.Column<double>(type: "double precision", nullable: false),
                    ChurnRate = table.Column<double>(type: "double precision", nullable: false),
                    LinesAdded = table.Column<int>(type: "integer", nullable: false),
                    LinesDeleted = table.Column<int>(type: "integer", nullable: false),
                    TimesRenamed = table.Column<int>(type: "integer", nullable: false),
                    TimesMoved = table.Column<int>(type: "integer", nullable: false),
                    StabilityScore = table.Column<double>(type: "double precision", nullable: false),
                    MaturityScore = table.Column<double>(type: "double precision", nullable: false),
                    SecurityHotspots = table.Column<int>(type: "integer", nullable: false),
                    VulnerabilityCount = table.Column<int>(type: "integer", nullable: false),
                    ContainsSensitiveData = table.Column<bool>(type: "boolean", nullable: false),
                    HasSecurityAnnotations = table.Column<bool>(type: "boolean", nullable: false),
                    IncomingDependencies = table.Column<int>(type: "integer", nullable: false),
                    OutgoingDependencies = table.Column<int>(type: "integer", nullable: false),
                    CouplingFactor = table.Column<double>(type: "double precision", nullable: false),
                    CohesionLevel = table.Column<double>(type: "double precision", nullable: false),
                    ExternalLibraryReferences = table.Column<int>(type: "integer", nullable: false),
                    DocumentationCoverage = table.Column<double>(type: "double precision", nullable: false),
                    HasUnitTests = table.Column<bool>(type: "boolean", nullable: false),
                    TestCoverage = table.Column<double>(type: "double precision", nullable: false),
                    TestCount = table.Column<int>(type: "integer", nullable: false),
                    HasDocumentationComments = table.Column<bool>(type: "boolean", nullable: false),
                    FileCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsGeneratedCode = table.Column<bool>(type: "boolean", nullable: false),
                    IsThirdParty = table.Column<bool>(type: "boolean", nullable: false),
                    IsBinaryFile = table.Column<bool>(type: "boolean", nullable: false),
                    IsConfigurationFile = table.Column<bool>(type: "boolean", nullable: false),
                    IsTestFile = table.Column<bool>(type: "boolean", nullable: false),
                    IsHotspot = table.Column<bool>(type: "boolean", nullable: false),
                    IsColdspot = table.Column<bool>(type: "boolean", nullable: false),
                    MaintenanceEffort = table.Column<double>(type: "double precision", nullable: false),
                    RefactoringPriority = table.Column<double>(type: "double precision", nullable: false),
                    BugProneness = table.Column<double>(type: "double precision", nullable: false),
                    ContributorBreakdown = table.Column<string>(type: "TEXT", nullable: true),
                    ChangePatterns = table.Column<string>(type: "TEXT", nullable: true),
                    DependencyGraph = table.Column<string>(type: "TEXT", nullable: true),
                    IssueHistory = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileMetrics_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepositoryMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalLinesOfCode = table.Column<int>(type: "integer", nullable: false),
                    EffectiveLinesOfCode = table.Column<int>(type: "integer", nullable: false),
                    CommentLines = table.Column<int>(type: "integer", nullable: false),
                    BlankLines = table.Column<int>(type: "integer", nullable: false),
                    CommentRatio = table.Column<double>(type: "double precision", nullable: false),
                    DuplicationPercentage = table.Column<double>(type: "double precision", nullable: false),
                    CodeSmells = table.Column<int>(type: "integer", nullable: false),
                    TechnicalDebtHours = table.Column<double>(type: "double precision", nullable: false),
                    MaintainabilityIndex = table.Column<double>(type: "double precision", nullable: false),
                    AverageCyclomaticComplexity = table.Column<double>(type: "double precision", nullable: false),
                    MaxCyclomaticComplexity = table.Column<int>(type: "integer", nullable: false),
                    AverageMethodLength = table.Column<double>(type: "double precision", nullable: false),
                    AverageClassSize = table.Column<double>(type: "double precision", nullable: false),
                    TotalMethods = table.Column<int>(type: "integer", nullable: false),
                    TotalClasses = table.Column<int>(type: "integer", nullable: false),
                    CognitiveComplexity = table.Column<double>(type: "double precision", nullable: false),
                    HalsteadVolume = table.Column<double>(type: "double precision", nullable: false),
                    HalsteadDifficulty = table.Column<double>(type: "double precision", nullable: false),
                    CommitsLastWeek = table.Column<int>(type: "integer", nullable: false),
                    CommitsLastMonth = table.Column<int>(type: "integer", nullable: false),
                    CommitsLastQuarter = table.Column<int>(type: "integer", nullable: false),
                    AverageCommitSize = table.Column<double>(type: "double precision", nullable: false),
                    FilesChangedLastWeek = table.Column<int>(type: "integer", nullable: false),
                    LinesAddedLastWeek = table.Column<int>(type: "integer", nullable: false),
                    LinesDeletedLastWeek = table.Column<int>(type: "integer", nullable: false),
                    DevelopmentVelocity = table.Column<double>(type: "double precision", nullable: false),
                    TotalFiles = table.Column<int>(type: "integer", nullable: false),
                    TotalDirectories = table.Column<int>(type: "integer", nullable: false),
                    RepositorySizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MaxDirectoryDepth = table.Column<int>(type: "integer", nullable: false),
                    AverageFileSize = table.Column<double>(type: "double precision", nullable: false),
                    BinaryFileCount = table.Column<int>(type: "integer", nullable: false),
                    TextFileCount = table.Column<int>(type: "integer", nullable: false),
                    LanguageDistribution = table.Column<string>(type: "TEXT", nullable: true),
                    FileTypeDistribution = table.Column<string>(type: "TEXT", nullable: true),
                    HourlyActivityPattern = table.Column<string>(type: "TEXT", nullable: true),
                    DailyActivityPattern = table.Column<string>(type: "TEXT", nullable: true),
                    LineCoveragePercentage = table.Column<double>(type: "double precision", nullable: false),
                    BranchCoveragePercentage = table.Column<double>(type: "double precision", nullable: false),
                    FunctionCoveragePercentage = table.Column<double>(type: "double precision", nullable: false),
                    TestToCodeRatio = table.Column<double>(type: "double precision", nullable: false),
                    TotalDependencies = table.Column<int>(type: "integer", nullable: false),
                    OutdatedDependencies = table.Column<int>(type: "integer", nullable: false),
                    VulnerableDependencies = table.Column<int>(type: "integer", nullable: false),
                    SecurityVulnerabilities = table.Column<int>(type: "integer", nullable: false),
                    CriticalVulnerabilities = table.Column<int>(type: "integer", nullable: false),
                    BuildSuccessRate = table.Column<double>(type: "double precision", nullable: false),
                    TestPassRate = table.Column<double>(type: "double precision", nullable: false),
                    QualityGateFailures = table.Column<int>(type: "integer", nullable: false),
                    DocumentationCoverage = table.Column<double>(type: "double precision", nullable: false),
                    ApiDocumentationCoverage = table.Column<double>(type: "double precision", nullable: false),
                    ReadmeWordCount = table.Column<int>(type: "integer", nullable: false),
                    WikiPageCount = table.Column<int>(type: "integer", nullable: false),
                    ActiveContributors = table.Column<int>(type: "integer", nullable: false),
                    TotalContributors = table.Column<int>(type: "integer", nullable: false),
                    BusFactor = table.Column<double>(type: "double precision", nullable: false),
                    CodeOwnershipConcentration = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepositoryMetrics_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_RepositoryId_Path",
                table: "Artifacts",
                columns: new[] { "RepositoryId", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactVersions_ArtifactId_ContentHash",
                table: "ArtifactVersions",
                columns: new[] { "ArtifactId", "ContentHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_AssignedByUserId",
                table: "AspNetUserRoles",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commits_RepositoryId_Sha",
                table: "Commits",
                columns: new[] { "RepositoryId", "Sha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContributorMetrics_RepositoryId_ContributorEmail_PeriodStart",
                table: "ContributorMetrics",
                columns: new[] { "RepositoryId", "ContributorEmail", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileMetrics_RepositoryId_FilePath",
                table: "FileMetrics",
                columns: new[] { "RepositoryId", "FilePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_OrganizationId",
                table: "Repositories",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_OwnerId",
                table: "Repositories",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_Url",
                table: "Repositories",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryMetrics_RepositoryId_MeasurementDate",
                table: "RepositoryMetrics",
                columns: new[] { "RepositoryId", "MeasurementDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artifacts");

            migrationBuilder.DropTable(
                name: "ArtifactVersions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "ContributorMetrics");

            migrationBuilder.DropTable(
                name: "FileMetrics");

            migrationBuilder.DropTable(
                name: "RepositoryMetrics");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
