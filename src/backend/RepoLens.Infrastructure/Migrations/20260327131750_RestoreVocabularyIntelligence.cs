using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RepoLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreVocabularyIntelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessConcepts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Domain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConceptType = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Keywords = table.Column<string>(type: "TEXT", nullable: false),
                    TechnicalMappings = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessPurposes = table.Column<string>(type: "TEXT", nullable: false),
                    RelatedTermIds = table.Column<string>(type: "TEXT", nullable: false),
                    Properties = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessConcepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessConcepts_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalTerms = table.Column<int>(type: "integer", nullable: false),
                    UniqueTerms = table.Column<int>(type: "integer", nullable: false),
                    BusinessTerms = table.Column<int>(type: "integer", nullable: false),
                    TechnicalTerms = table.Column<int>(type: "integer", nullable: false),
                    DomainSpecificTerms = table.Column<int>(type: "integer", nullable: false),
                    AverageRelevanceScore = table.Column<double>(type: "double precision", nullable: false),
                    VocabularyDensity = table.Column<double>(type: "double precision", nullable: false),
                    BusinessTechnicalRatio = table.Column<double>(type: "double precision", nullable: false),
                    LanguageDistribution = table.Column<string>(type: "TEXT", nullable: false),
                    DomainDistribution = table.Column<string>(type: "TEXT", nullable: false),
                    SourceDistribution = table.Column<string>(type: "TEXT", nullable: false),
                    TopDomains = table.Column<string>(type: "TEXT", nullable: false),
                    EmergingTerms = table.Column<string>(type: "TEXT", nullable: false),
                    DeprecatedTerms = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyStats_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    Term = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedTerm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TermType = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    RelevanceScore = table.Column<double>(type: "double precision", nullable: false),
                    BusinessRelevance = table.Column<double>(type: "double precision", nullable: false),
                    TechnicalRelevance = table.Column<double>(type: "double precision", nullable: false),
                    Context = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Definition = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Domain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Synonyms = table.Column<string>(type: "TEXT", nullable: false),
                    RelatedTerms = table.Column<string>(type: "TEXT", nullable: false),
                    UsageExamples = table.Column<string>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyTerms_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VocabularyTermId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StartLine = table.Column<int>(type: "integer", nullable: false),
                    EndLine = table.Column<int>(type: "integer", nullable: false),
                    StartColumn = table.Column<int>(type: "integer", nullable: false),
                    EndColumn = table.Column<int>(type: "integer", nullable: false),
                    ContextType = table.Column<string>(type: "text", nullable: false),
                    ContextDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SurroundingCode = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FoundAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyLocations_VocabularyTerms_VocabularyTermId",
                        column: x => x.VocabularyTermId,
                        principalTable: "VocabularyTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyTermRelationships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromTermId = table.Column<int>(type: "integer", nullable: false),
                    ToTermId = table.Column<int>(type: "integer", nullable: false),
                    RelationshipType = table.Column<string>(type: "text", nullable: false),
                    Strength = table.Column<double>(type: "double precision", nullable: false),
                    CoOccurrenceCount = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyTermRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyTermRelationships_VocabularyTerms_FromTermId",
                        column: x => x.FromTermId,
                        principalTable: "VocabularyTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocabularyTermRelationships_VocabularyTerms_ToTermId",
                        column: x => x.ToTermId,
                        principalTable: "VocabularyTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessConcepts_ConceptType",
                table: "BusinessConcepts",
                column: "ConceptType");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessConcepts_Domain",
                table: "BusinessConcepts",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessConcepts_RepositoryId",
                table: "BusinessConcepts",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyLocations_FilePath",
                table: "VocabularyLocations",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyLocations_VocabularyTermId",
                table: "VocabularyLocations",
                column: "VocabularyTermId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyStats_RepositoryId",
                table: "VocabularyStats",
                column: "RepositoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTermRelationships_FromTermId_ToTermId",
                table: "VocabularyTermRelationships",
                columns: new[] { "FromTermId", "ToTermId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTermRelationships_RelationshipType",
                table: "VocabularyTermRelationships",
                column: "RelationshipType");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTermRelationships_ToTermId",
                table: "VocabularyTermRelationships",
                column: "ToTermId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTerms_Domain",
                table: "VocabularyTerms",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTerms_RelevanceScore",
                table: "VocabularyTerms",
                column: "RelevanceScore");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTerms_RepositoryId_NormalizedTerm",
                table: "VocabularyTerms",
                columns: new[] { "RepositoryId", "NormalizedTerm" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTerms_Source",
                table: "VocabularyTerms",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyTerms_TermType",
                table: "VocabularyTerms",
                column: "TermType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessConcepts");

            migrationBuilder.DropTable(
                name: "VocabularyLocations");

            migrationBuilder.DropTable(
                name: "VocabularyStats");

            migrationBuilder.DropTable(
                name: "VocabularyTermRelationships");

            migrationBuilder.DropTable(
                name: "VocabularyTerms");
        }
    }
}
