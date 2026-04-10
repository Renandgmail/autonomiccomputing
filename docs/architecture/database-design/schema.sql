-- RepoLens Database Schema
-- PostgreSQL 15+ Compatible
-- Generated from Entity Framework Core Model Snapshot

-- =============================================
-- REPOLENS COMPLETE DATABASE SCHEMA
-- =============================================

-- Enable UUID extension if not already enabled
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- IDENTITY AND AUTHENTICATION TABLES
-- =============================================

-- Organizations table
CREATE TABLE "Organizations" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000),
    "Website" VARCHAR(500),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "Settings" TEXT NOT NULL DEFAULT '{}',
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Users table (ASP.NET Identity)
CREATE TABLE "AspNetUsers" (
    "Id" SERIAL PRIMARY KEY,
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256) NOT NULL,
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockoutEnd" TIMESTAMP WITH TIME ZONE,
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "LastLoginAt" TIMESTAMP WITH TIME ZONE,
    "ProfileImageUrl" VARCHAR(500),
    "TimeZone" VARCHAR(50),
    "Preferences" TEXT NOT NULL DEFAULT '{}',
    "OrganizationId" INTEGER,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_AspNetUsers_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations"("Id") ON DELETE SET NULL
);

-- Roles table (ASP.NET Identity)
CREATE TABLE "AspNetRoles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(256) NOT NULL,
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT,
    "Description" VARCHAR(500),
    "IsSystemRole" BOOLEAN NOT NULL DEFAULT FALSE,
    "Permissions" TEXT NOT NULL DEFAULT '[]',
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- User-Role mapping table
CREATE TABLE "AspNetUserRoles" (
    "UserId" INTEGER NOT NULL,
    "RoleId" INTEGER NOT NULL,
    "AssignedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "AssignedByUserId" INTEGER,
    PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_AssignedByUserId" FOREIGN KEY ("AssignedByUserId") REFERENCES "AspNetUsers"("Id") ON DELETE SET NULL
);

-- Additional Identity tables
CREATE TABLE "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" INTEGER NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" INTEGER NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" INTEGER NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- =============================================
-- REPOSITORY MANAGEMENT TABLES
-- =============================================

-- Repositories table
CREATE TABLE "Repositories" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000),
    "Url" VARCHAR(1000) NOT NULL,
    "Type" INTEGER NOT NULL DEFAULT 0,
    "ProviderType" INTEGER NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "IsPrivate" BOOLEAN NOT NULL DEFAULT FALSE,
    "DefaultBranch" VARCHAR(100),
    "Username" VARCHAR(100),
    "AccessToken" VARCHAR(500),
    "AuthTokenReference" TEXT,
    "TokenExpiresAt" TIMESTAMP WITH TIME ZONE,
    "AutoSync" BOOLEAN NOT NULL DEFAULT TRUE,
    "SyncIntervalMinutes" INTEGER NOT NULL DEFAULT 60,
    "LastSyncAt" TIMESTAMP WITH TIME ZONE,
    "LastSyncCommit" TEXT,
    "SyncErrorMessage" VARCHAR(2000),
    "LastAnalysisAt" TIMESTAMP WITH TIME ZONE,
    "ScanStatus" TEXT,
    "ScanErrorMessage" TEXT,
    "TotalFiles" INTEGER NOT NULL DEFAULT 0,
    "TotalLines" INTEGER NOT NULL DEFAULT 0,
    "OwnerId" INTEGER,
    "OrganizationId" INTEGER,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    CONSTRAINT "FK_Repositories_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "AspNetUsers"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Repositories_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations"("Id") ON DELETE SET NULL
);

-- Repository Files table
CREATE TABLE "RepositoryFiles" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "FilePath" VARCHAR(1000) NOT NULL,
    "FileName" VARCHAR(255) NOT NULL,
    "FileExtension" VARCHAR(20) NOT NULL,
    "Language" VARCHAR(50) NOT NULL,
    "FileSize" BIGINT NOT NULL DEFAULT 0,
    "LineCount" INTEGER NOT NULL DEFAULT 0,
    "LastModified" TIMESTAMP WITH TIME ZONE NOT NULL,
    "FileHash" VARCHAR(64) NOT NULL,
    "ProcessingStatus" TEXT NOT NULL DEFAULT 'Pending',
    "ProcessingTime" INTEGER,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_RepositoryFiles_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- Commits table
CREATE TABLE "Commits" (
    "Sha" TEXT PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "Author" TEXT NOT NULL,
    "Message" TEXT NOT NULL,
    "Timestamp" TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT "FK_Commits_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- =============================================
-- CODE INTELLIGENCE TABLES
-- =============================================

-- Code Elements table (AST analysis)
CREATE TABLE "CodeElements" (
    "Id" SERIAL PRIMARY KEY,
    "FileId" INTEGER NOT NULL,
    "ElementType" TEXT NOT NULL,
    "Name" VARCHAR(500) NOT NULL,
    "FullyQualifiedName" VARCHAR(1000),
    "StartLine" INTEGER NOT NULL,
    "EndLine" INTEGER NOT NULL,
    "Signature" TEXT,
    "AccessModifier" VARCHAR(20),
    "IsStatic" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsAsync" BOOLEAN NOT NULL DEFAULT FALSE,
    "ReturnType" VARCHAR(200),
    "Parameters" TEXT DEFAULT '[]',
    "Documentation" TEXT,
    "Complexity" INTEGER,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_CodeElements_RepositoryFiles_FileId" FOREIGN KEY ("FileId") REFERENCES "RepositoryFiles"("Id") ON DELETE CASCADE
);

-- =============================================
-- VOCABULARY INTELLIGENCE TABLES
-- =============================================

-- Vocabulary Terms table
CREATE TABLE "VocabularyTerms" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "Term" VARCHAR(200) NOT NULL,
    "NormalizedTerm" VARCHAR(200) NOT NULL,
    "TermType" TEXT NOT NULL,
    "Source" TEXT NOT NULL,
    "Language" VARCHAR(50) NOT NULL,
    "Frequency" INTEGER NOT NULL DEFAULT 1,
    "RelevanceScore" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "BusinessRelevance" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "TechnicalRelevance" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "Context" VARCHAR(2000),
    "Definition" VARCHAR(1000),
    "Domain" VARCHAR(100),
    "Synonyms" TEXT NOT NULL DEFAULT '[]',
    "RelatedTerms" TEXT NOT NULL DEFAULT '[]',
    "UsageExamples" TEXT NOT NULL DEFAULT '[]',
    "Metadata" TEXT NOT NULL DEFAULT '{}',
    "FirstSeen" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastSeen" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_VocabularyTerms_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- Vocabulary Locations table (where terms are found)
CREATE TABLE "VocabularyLocations" (
    "Id" SERIAL PRIMARY KEY,
    "VocabularyTermId" INTEGER NOT NULL,
    "FilePath" VARCHAR(1000) NOT NULL,
    "StartLine" INTEGER NOT NULL,
    "EndLine" INTEGER NOT NULL,
    "StartColumn" INTEGER NOT NULL,
    "EndColumn" INTEGER NOT NULL,
    "ContextType" TEXT NOT NULL,
    "ContextDescription" VARCHAR(500) NOT NULL,
    "SurroundingCode" VARCHAR(2000),
    "FoundAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_VocabularyLocations_VocabularyTerms_VocabularyTermId" FOREIGN KEY ("VocabularyTermId") REFERENCES "VocabularyTerms"("Id") ON DELETE CASCADE
);

-- Vocabulary Term Relationships table
CREATE TABLE "VocabularyTermRelationships" (
    "Id" SERIAL PRIMARY KEY,
    "FromTermId" INTEGER NOT NULL,
    "ToTermId" INTEGER NOT NULL,
    "RelationshipType" TEXT NOT NULL,
    "Strength" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CoOccurrenceCount" INTEGER NOT NULL DEFAULT 0,
    "Context" VARCHAR(500),
    "Evidence" TEXT NOT NULL DEFAULT '[]',
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_VocabularyTermRelationships_VocabularyTerms_FromTermId" FOREIGN KEY ("FromTermId") REFERENCES "VocabularyTerms"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_VocabularyTermRelationships_VocabularyTerms_ToTermId" FOREIGN KEY ("ToTermId") REFERENCES "VocabularyTerms"("Id") ON DELETE CASCADE
);

-- Business Concepts table
CREATE TABLE "BusinessConcepts" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000) NOT NULL,
    "Domain" VARCHAR(100) NOT NULL,
    "ConceptType" TEXT NOT NULL,
    "Confidence" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "Keywords" TEXT NOT NULL DEFAULT '[]',
    "TechnicalMappings" TEXT NOT NULL DEFAULT '[]',
    "BusinessPurposes" TEXT NOT NULL DEFAULT '[]',
    "RelatedTermIds" TEXT NOT NULL DEFAULT '[]',
    "Properties" TEXT NOT NULL DEFAULT '{}',
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_BusinessConcepts_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- Vocabulary Statistics table
CREATE TABLE "VocabularyStats" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "CalculatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "TotalTerms" INTEGER NOT NULL DEFAULT 0,
    "UniqueTerms" INTEGER NOT NULL DEFAULT 0,
    "BusinessTerms" INTEGER NOT NULL DEFAULT 0,
    "TechnicalTerms" INTEGER NOT NULL DEFAULT 0,
    "DomainSpecificTerms" INTEGER NOT NULL DEFAULT 0,
    "AverageRelevanceScore" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "VocabularyDensity" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "BusinessTechnicalRatio" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "LanguageDistribution" TEXT NOT NULL DEFAULT '{}',
    "DomainDistribution" TEXT NOT NULL DEFAULT '{}',
    "SourceDistribution" TEXT NOT NULL DEFAULT '{}',
    "TopDomains" TEXT NOT NULL DEFAULT '[]',
    "EmergingTerms" TEXT NOT NULL DEFAULT '[]',
    "DeprecatedTerms" TEXT NOT NULL DEFAULT '[]',
    CONSTRAINT "FK_VocabularyStats_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- =============================================
-- METRICS AND ANALYTICS TABLES
-- =============================================

-- Repository Metrics table
CREATE TABLE "RepositoryMetrics" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "MeasurementDate" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "TotalFiles" INTEGER NOT NULL DEFAULT 0,
    "TotalDirectories" INTEGER NOT NULL DEFAULT 0,
    "TotalLinesOfCode" INTEGER NOT NULL DEFAULT 0,
    "EffectiveLinesOfCode" INTEGER NOT NULL DEFAULT 0,
    "BlankLines" INTEGER NOT NULL DEFAULT 0,
    "CommentLines" INTEGER NOT NULL DEFAULT 0,
    "CommentRatio" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "BinaryFileCount" INTEGER NOT NULL DEFAULT 0,
    "TextFileCount" INTEGER NOT NULL DEFAULT 0,
    "MaxDirectoryDepth" INTEGER NOT NULL DEFAULT 0,
    "RepositorySizeBytes" BIGINT NOT NULL DEFAULT 0,
    "AverageFileSize" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "LanguageDistribution" TEXT DEFAULT '{}',
    "FileTypeDistribution" TEXT DEFAULT '{}',
    "TotalContributors" INTEGER NOT NULL DEFAULT 0,
    "ActiveContributors" INTEGER NOT NULL DEFAULT 0,
    "TotalClasses" INTEGER NOT NULL DEFAULT 0,
    "TotalMethods" INTEGER NOT NULL DEFAULT 0,
    "AverageClassSize" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "AverageMethodLength" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "AverageCyclomaticComplexity" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "MaxCyclomaticComplexity" INTEGER NOT NULL DEFAULT 0,
    "CognitiveComplexity" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "HalsteadVolume" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "HalsteadDifficulty" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "MaintainabilityIndex" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "TechnicalDebtHours" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CodeSmells" INTEGER NOT NULL DEFAULT 0,
    "SecurityVulnerabilities" INTEGER NOT NULL DEFAULT 0,
    "CriticalVulnerabilities" INTEGER NOT NULL DEFAULT 0,
    "BusFactor" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CodeOwnershipConcentration" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CommitsLastWeek" INTEGER NOT NULL DEFAULT 0,
    "CommitsLastMonth" INTEGER NOT NULL DEFAULT 0,
    "CommitsLastQuarter" INTEGER NOT NULL DEFAULT 0,
    "FilesChangedLastWeek" INTEGER NOT NULL DEFAULT 0,
    "LinesAddedLastWeek" INTEGER NOT NULL DEFAULT 0,
    "LinesDeletedLastWeek" INTEGER NOT NULL DEFAULT 0,
    "DevelopmentVelocity" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "AverageCommitSize" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "DuplicationPercentage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "TestToCodeRatio" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "TestPassRate" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "LineCoveragePercentage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "BranchCoveragePercentage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "FunctionCoveragePercentage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "BuildSuccessRate" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "DocumentationCoverage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "ApiDocumentationCoverage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "ReadmeWordCount" INTEGER NOT NULL DEFAULT 0,
    "WikiPageCount" INTEGER NOT NULL DEFAULT 0,
    "TotalDependencies" INTEGER NOT NULL DEFAULT 0,
    "OutdatedDependencies" INTEGER NOT NULL DEFAULT 0,
    "VulnerableDependencies" INTEGER NOT NULL DEFAULT 0,
    "QualityGateFailures" INTEGER NOT NULL DEFAULT 0,
    "HourlyActivityPattern" TEXT DEFAULT '{}',
    "DailyActivityPattern" TEXT DEFAULT '{}',
    CONSTRAINT "FK_RepositoryMetrics_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- File Metrics table
CREATE TABLE "FileMetrics" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "FilePath" VARCHAR(1000) NOT NULL,
    "FileName" VARCHAR(255) NOT NULL,
    "FileExtension" VARCHAR(50) NOT NULL,
    "PrimaryLanguage" VARCHAR(100) NOT NULL,
    "FileCategory" VARCHAR(100) NOT NULL,
    "FileSizeBytes" BIGINT NOT NULL DEFAULT 0,
    "LineCount" INTEGER NOT NULL DEFAULT 0,
    "EffectiveLineCount" INTEGER NOT NULL DEFAULT 0,
    "BlankLineCount" INTEGER NOT NULL DEFAULT 0,
    "CommentLineCount" INTEGER NOT NULL DEFAULT 0,
    "CommentDensity" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "FunctionCount" INTEGER NOT NULL DEFAULT 0,
    "ClassCount" INTEGER NOT NULL DEFAULT 0,
    "MethodCount" INTEGER NOT NULL DEFAULT 0,
    "AverageMethodLength" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "MaxMethodLength" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CyclomaticComplexity" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CognitiveComplexity" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "NestingDepth" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "MaintainabilityIndex" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "TechnicalDebtMinutes" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "RefactoringPriority" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "DuplicationLines" INTEGER NOT NULL DEFAULT 0,
    "DuplicationPercentage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CohesionLevel" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CouplingFactor" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "IncomingDependencies" INTEGER NOT NULL DEFAULT 0,
    "OutgoingDependencies" INTEGER NOT NULL DEFAULT 0,
    "ExternalLibraryReferences" INTEGER NOT NULL DEFAULT 0,
    "TestCount" INTEGER NOT NULL DEFAULT 0,
    "TestCoverage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "DocumentationCoverage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "SecurityHotspots" INTEGER NOT NULL DEFAULT 0,
    "VulnerabilityCount" INTEGER NOT NULL DEFAULT 0,
    "CodeSmellCount" INTEGER NOT NULL DEFAULT 0,
    "MajorIssues" INTEGER NOT NULL DEFAULT 0,
    "MinorIssues" INTEGER NOT NULL DEFAULT 0,
    "CriticalIssues" INTEGER NOT NULL DEFAULT 0,
    "BugProneness" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "ChurnRate" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "ChangeFrequency" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "StabilityScore" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "MaturityScore" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "MaintenanceEffort" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "TotalCommits" INTEGER NOT NULL DEFAULT 0,
    "UniqueContributors" INTEGER NOT NULL DEFAULT 0,
    "CommitsLastMonth" INTEGER NOT NULL DEFAULT 0,
    "CommitsLastQuarter" INTEGER NOT NULL DEFAULT 0,
    "LinesAdded" INTEGER NOT NULL DEFAULT 0,
    "LinesDeleted" INTEGER NOT NULL DEFAULT 0,
    "FirstCommit" TIMESTAMP WITH TIME ZONE,
    "LastCommit" TIMESTAMP WITH TIME ZONE,
    "TimesRenamed" INTEGER NOT NULL DEFAULT 0,
    "TimesMoved" INTEGER NOT NULL DEFAULT 0,
    "IsTestFile" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsGeneratedCode" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsBinaryFile" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsConfigurationFile" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsThirdParty" BOOLEAN NOT NULL DEFAULT FALSE,
    "HasUnitTests" BOOLEAN NOT NULL DEFAULT FALSE,
    "HasDocumentationComments" BOOLEAN NOT NULL DEFAULT FALSE,
    "HasSecurityAnnotations" BOOLEAN NOT NULL DEFAULT FALSE,
    "ContainsSensitiveData" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsHotspot" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsColdspot" BOOLEAN NOT NULL DEFAULT FALSE,
    "LastAnalyzed" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ChangePatterns" TEXT DEFAULT '{}',
    "DependencyGraph" TEXT DEFAULT '{}',
    "ContributorBreakdown" TEXT DEFAULT '{}',
    "IssueHistory" TEXT DEFAULT '[]',
    "LanguageContributions" TEXT DEFAULT '{}',
    CONSTRAINT "FK_FileMetrics_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- Contributor Metrics table
CREATE TABLE "ContributorMetrics" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "ContributorName" VARCHAR(255) NOT NULL,
    "ContributorEmail" VARCHAR(255) NOT NULL,
    "PeriodStart" TIMESTAMP WITH TIME ZONE NOT NULL,
    "PeriodEnd" TIMESTAMP WITH TIME ZONE NOT NULL,
    "CommitCount" INTEGER NOT NULL DEFAULT 0,
    "LinesAdded" INTEGER NOT NULL DEFAULT 0,
    "LinesDeleted" INTEGER NOT NULL DEFAULT 0,
    "LinesModified" INTEGER NOT NULL DEFAULT 0,
    "FilesAdded" INTEGER NOT NULL DEFAULT 0,
    "FilesModified" INTEGER NOT NULL DEFAULT 0,
    "FilesDeleted" INTEGER NOT NULL DEFAULT 0,
    "UniqueFilesTouched" INTEGER NOT NULL DEFAULT 0,
    "ContributionPercentage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CodeOwnershipPercentage" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "OwnedFiles" INTEGER NOT NULL DEFAULT 0,
    "AverageCommitSize" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CommitFrequency" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CommitMessageQualityScore" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "AvgCommitMessageLength" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "BugFixCommits" INTEGER NOT NULL DEFAULT 0,
    "FeatureCommits" INTEGER NOT NULL DEFAULT 0,
    "RefactoringCommits" INTEGER NOT NULL DEFAULT 0,
    "DocumentationCommits" INTEGER NOT NULL DEFAULT 0,
    "FirstContribution" TIMESTAMP WITH TIME ZONE,
    "LastContribution" TIMESTAMP WITH TIME ZONE,
    "DaysActive" INTEGER NOT NULL DEFAULT 0,
    "WorkingDays" INTEGER NOT NULL DEFAULT 0,
    "IsNewContributor" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsCoreContributor" BOOLEAN NOT NULL DEFAULT FALSE,
    "CurrentCommitStreak" INTEGER NOT NULL DEFAULT 0,
    "LongestCommitStreak" INTEGER NOT NULL DEFAULT 0,
    "RetentionScore" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "PullRequestsCreated" INTEGER NOT NULL DEFAULT 0,
    "PullRequestsReviewed" INTEGER NOT NULL DEFAULT 0,
    "CodeReviewComments" INTEGER NOT NULL DEFAULT 0,
    "IssuesCreated" INTEGER NOT NULL DEFAULT 0,
    "IssuesResolved" INTEGER NOT NULL DEFAULT 0,
    "MentionedInCommits" INTEGER NOT NULL DEFAULT 0,
    "HourlyActivityPattern" TEXT DEFAULT '{}',
    "WeeklyActivityPattern" TEXT DEFAULT '{}',
    "MonthlyActivityPattern" TEXT DEFAULT '{}',
    "LanguageContributions" TEXT DEFAULT '{}',
    "FileTypeContributions" TEXT DEFAULT '{}',
    CONSTRAINT "FK_ContributorMetrics_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- =============================================
-- ARTIFACT STORAGE TABLES
-- =============================================

-- Artifacts table (for pattern storage)
CREATE TABLE "Artifacts" (
    "Id" SERIAL PRIMARY KEY,
    "RepositoryId" INTEGER NOT NULL,
    "Path" TEXT NOT NULL,
    CONSTRAINT "FK_Artifacts_Repositories_RepositoryId" FOREIGN KEY ("RepositoryId") REFERENCES "Repositories"("Id") ON DELETE CASCADE
);

-- Artifact Versions table
CREATE TABLE "ArtifactVersions" (
    "Id" SERIAL PRIMARY KEY,
    "ArtifactId" INTEGER NOT NULL,
    "CommitSha" TEXT NOT NULL,
    "ContentHash" TEXT NOT NULL,
    "StoredAt" TEXT NOT NULL,
    "Metadata" TEXT NOT NULL DEFAULT '{}',
    CONSTRAINT "FK_ArtifactVersions_Artifacts_ArtifactId" FOREIGN KEY ("ArtifactId") REFERENCES "Artifacts"("Id") ON DELETE CASCADE
);

-- =============================================
-- INDEXES FOR PERFORMANCE OPTIMIZATION
-- =============================================

-- Authentication and User Management
CREATE UNIQUE INDEX "IX_AspNetUsers_Email" ON "AspNetUsers" ("Email");
CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE INDEX "IX_AspNetUserRoles_AssignedByUserId" ON "AspNetUserRoles" ("AssignedByUserId");
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

-- Repository Management
CREATE UNIQUE INDEX "IX_Repositories_Url" ON "Repositories" ("Url");
CREATE INDEX "IX_Repositories_OwnerId" ON "Repositories" ("OwnerId");
CREATE INDEX "IX_Repositories_OrganizationId" ON "Repositories" ("OrganizationId");
CREATE UNIQUE INDEX "IX_RepositoryFiles_RepositoryId_FilePath" ON "RepositoryFiles" ("RepositoryId", "FilePath");
CREATE INDEX "IX_RepositoryFiles_Language" ON "RepositoryFiles" ("Language");
CREATE INDEX "IX_RepositoryFiles_FileExtension" ON "RepositoryFiles" ("FileExtension");
CREATE INDEX "IX_RepositoryFiles_ProcessingStatus" ON "RepositoryFiles" ("ProcessingStatus");
CREATE UNIQUE INDEX "IX_Commits_RepositoryId_Sha" ON "Commits" ("RepositoryId", "Sha");

-- Code Intelligence
CREATE INDEX "IX_CodeElements_FileId" ON "CodeElements" ("FileId");
CREATE INDEX "IX_CodeElements_ElementType" ON "CodeElements" ("ElementType");
CREATE INDEX "IX_CodeElements_Name" ON "CodeElements" ("Name");

-- Vocabulary Intelligence
CREATE UNIQUE INDEX "IX_VocabularyTerms_RepositoryId_NormalizedTerm" ON "VocabularyTerms" ("RepositoryId", "NormalizedTerm");
CREATE INDEX "IX_VocabularyTerms_TermType" ON "VocabularyTerms" ("TermType");
CREATE INDEX "IX_VocabularyTerms_Source" ON "VocabularyTerms" ("Source");
CREATE INDEX "IX_VocabularyTerms_Domain" ON "VocabularyTerms" ("Domain");
CREATE INDEX "IX_VocabularyTerms_RelevanceScore" ON "VocabularyTerms" ("RelevanceScore");
CREATE INDEX "IX_VocabularyLocations_VocabularyTermId" ON "VocabularyLocations" ("VocabularyTermId");
CREATE INDEX "IX_VocabularyLocations_FilePath" ON "VocabularyLocations" ("FilePath");
CREATE UNIQUE INDEX "IX_VocabularyTermRelationships_FromTermId_ToTermId" ON "VocabularyTermRelationships" ("FromTermId", "ToTermId");
CREATE INDEX "IX_VocabularyTermRelationships_ToTermId" ON "VocabularyTermRelationships" ("ToTermId");
CREATE INDEX "IX_VocabularyTermRelationships_RelationshipType" ON "VocabularyTermRelationships" ("RelationshipType");
CREATE INDEX "IX_BusinessConcepts_RepositoryId" ON "BusinessConcepts" ("RepositoryId");
CREATE INDEX "IX_BusinessConcepts_Domain" ON "BusinessConcepts" ("Domain");
CREATE INDEX "IX_BusinessConcepts_ConceptType" ON "BusinessConcepts" ("ConceptType");
CREATE UNIQUE INDEX "IX_VocabularyStats_RepositoryId" ON "VocabularyStats" ("RepositoryId");

-- Analytics and Metrics
CREATE UNIQUE INDEX "IX_RepositoryMetrics_RepositoryId_MeasurementDate" ON "RepositoryMetrics" ("RepositoryId", "MeasurementDate");
CREATE UNIQUE INDEX "IX_FileMetrics_RepositoryId_FilePath" ON "FileMetrics" ("RepositoryId", "FilePath");
CREATE UNIQUE INDEX "IX_ContributorMetrics_RepositoryId_ContributorEmail_PeriodStart" ON "ContributorMetrics" ("RepositoryId", "ContributorEmail", "PeriodStart");

-- Artifact Storage
CREATE UNIQUE INDEX "IX_Artifacts_RepositoryId_Path" ON "Artifacts" ("RepositoryId", "Path");
CREATE UNIQUE INDEX "IX_ArtifactVersions_ArtifactId_ContentHash" ON "ArtifactVersions" ("ArtifactId", "ContentHash");

-- Full-text search indexes for natural language queries
CREATE INDEX "IX_CodeElements_Signature_GIN" ON "CodeElements" USING gin(to_tsvector('english', "Signature"));
CREATE INDEX "IX_CodeElements_Documentation_GIN" ON "CodeElements" USING gin(to_tsvector('english', "Documentation"));
CREATE INDEX "IX_VocabularyTerms_Term_GIN" ON "VocabularyTerms" USING gin(to_tsvector('english', "Term"));

-- =============================================
-- DEFAULT DATA INSERTION
-- =============================================

-- Insert default admin role
INSERT INTO "AspNetRoles" ("Name", "NormalizedName", "Description", "IsSystemRole", "Permissions")
VALUES ('Admin', 'ADMIN', 'System Administrator with full access', TRUE, '["*"]');

-- Insert default user role  
INSERT INTO "AspNetRoles" ("Name", "NormalizedName", "Description", "IsSystemRole", "Permissions")
VALUES ('User', 'USER', 'Standard user with limited access', TRUE, '["read", "analyze"]');

-- Insert default viewer role
INSERT INTO "AspNetRoles" ("Name", "NormalizedName", "Description", "IsSystemRole", "Permissions")
VALUES ('Viewer', 'VIEWER', 'Read-only access to repositories', TRUE, '["read"]');

-- =============================================
-- SCHEMA INFORMATION
-- =============================================

-- Schema version tracking
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Insert migration history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
    ('20260325174836_InitialCreatePostgreSQL', '8.0.10'),
    ('20260326144136_AddProviderTypeToRepositories', '8.0.10'),
    ('20260327084415_AddCodeIntelligenceEntities', '8.0.10'),
    ('20260327131750_RestoreVocabularyIntelligence', '8.0.10');

-- =============================================
-- COMMENTS AND DOCUMENTATION
-- =============================================

COMMENT ON TABLE "Repositories" IS 'Core repository information and metadata';
COMMENT ON TABLE "RepositoryFiles" IS 'Individual files within repositories with processing status';
COMMENT ON TABLE "CodeElements" IS 'Parsed code elements (classes, methods, functions) from AST analysis';
COMMENT ON TABLE "VocabularyTerms" IS 'Domain-specific vocabulary extracted from codebases';
COMMENT ON TABLE "VocabularyLocations" IS 'Precise locations where vocabulary terms are found';
COMMENT ON TABLE "VocabularyTermRelationships" IS 'Relationships and connections between vocabulary terms';
COMMENT ON TABLE "BusinessConcepts" IS 'High-level business concepts extracted from code';
COMMENT ON TABLE "VocabularyStats" IS 'Aggregated statistics about repository vocabulary';
COMMENT ON TABLE "RepositoryMetrics" IS 'Comprehensive analytics and metrics for repositories';
COMMENT ON TABLE "FileMetrics" IS 'Detailed file-level metrics and quality indicators';
COMMENT ON TABLE "ContributorMetrics" IS 'Developer contribution patterns and productivity metrics';

-- End of Schema
-- Total Tables: 23 core tables + 7 Identity tables = 30 tables
-- Features: Authentication, Repository Management, Code Intelligence, Vocabulary Analysis, Metrics & Analytics
