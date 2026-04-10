using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RepoLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddASTAnalysisTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASTRepositoryAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTRepositoryAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTRepositoryAnalyses_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TargetFile = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DependencyType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceElement = table.Column<string>(type: "text", nullable: true),
                    TargetElement = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    IsCircular = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTDependencies_ASTRepositoryAnalyses_RepositoryAnalysisId",
                        column: x => x.RepositoryAnalysisId,
                        principalTable: "ASTRepositoryAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTFileAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsSupported = table.Column<bool>(type: "boolean", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTFileAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTFileAnalyses_ASTRepositoryAnalyses_RepositoryAnalysisId",
                        column: x => x.RepositoryAnalysisId,
                        principalTable: "ASTRepositoryAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    TotalFiles = table.Column<int>(type: "integer", nullable: false),
                    TotalLinesOfCode = table.Column<int>(type: "integer", nullable: false),
                    TotalStatements = table.Column<int>(type: "integer", nullable: false),
                    TotalClasses = table.Column<int>(type: "integer", nullable: false),
                    TotalMethods = table.Column<int>(type: "integer", nullable: false),
                    AverageComplexity = table.Column<double>(type: "double precision", nullable: false),
                    MaxComplexity = table.Column<int>(type: "integer", nullable: false),
                    TotalIssues = table.Column<int>(type: "integer", nullable: false),
                    CriticalIssues = table.Column<int>(type: "integer", nullable: false),
                    HighIssues = table.Column<int>(type: "integer", nullable: false),
                    MediumIssues = table.Column<int>(type: "integer", nullable: false),
                    LowIssues = table.Column<int>(type: "integer", nullable: false),
                    CodeDuplicationPercentage = table.Column<double>(type: "double precision", nullable: false),
                    CircularDependencies = table.Column<int>(type: "integer", nullable: false),
                    TechnicalDebtHours = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTMetrics_ASTRepositoryAnalyses_RepositoryAnalysisId",
                        column: x => x.RepositoryAnalysisId,
                        principalTable: "ASTRepositoryAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DuplicateCodeBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StartLine = table.Column<int>(type: "integer", nullable: false),
                    EndLine = table.Column<int>(type: "integer", nullable: false),
                    CodeBlock = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SimilarityScore = table.Column<int>(type: "integer", nullable: false),
                    LinesOfCode = table.Column<int>(type: "integer", nullable: false),
                    DuplicateType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateCodeBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuplicateCodeBlocks_ASTRepositoryAnalyses_RepositoryAnalysi~",
                        column: x => x.RepositoryAnalysisId,
                        principalTable: "ASTRepositoryAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StartLine = table.Column<int>(type: "integer", nullable: false),
                    EndLine = table.Column<int>(type: "integer", nullable: false),
                    AccessModifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAbstract = table.Column<bool>(type: "boolean", nullable: false),
                    IsInterface = table.Column<bool>(type: "boolean", nullable: false),
                    BaseClass = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Interfaces = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTClasses_ASTFileAnalyses_FileAnalysisId",
                        column: x => x.FileAnalysisId,
                        principalTable: "ASTFileAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTExports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Line = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTExports_ASTFileAnalyses_FileAnalysisId",
                        column: x => x.FileAnalysisId,
                        principalTable: "ASTFileAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTFileMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    LinesOfCode = table.Column<int>(type: "integer", nullable: false),
                    Statements = table.Column<int>(type: "integer", nullable: false),
                    Classes = table.Column<int>(type: "integer", nullable: false),
                    Methods = table.Column<int>(type: "integer", nullable: false),
                    Complexity = table.Column<double>(type: "double precision", nullable: false),
                    Issues = table.Column<int>(type: "integer", nullable: false),
                    QualityScore = table.Column<double>(type: "double precision", nullable: false),
                    QualityTrend = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTFileMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTFileMetrics_ASTFileAnalyses_FileAnalysisId",
                        column: x => x.FileAnalysisId,
                        principalTable: "ASTFileAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    Module = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Alias = table.Column<string>(type: "text", nullable: true),
                    ImportedSymbols = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefaultImport = table.Column<bool>(type: "boolean", nullable: false),
                    IsNamespaceImport = table.Column<bool>(type: "boolean", nullable: false),
                    Line = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTImports_ASTFileAnalyses_FileAnalysisId",
                        column: x => x.FileAnalysisId,
                        principalTable: "ASTFileAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTStatements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    StatementId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Line = table.Column<int>(type: "integer", nullable: false),
                    Column = table.Column<int>(type: "integer", nullable: false),
                    StartPosition = table.Column<int>(type: "integer", nullable: false),
                    EndPosition = table.Column<int>(type: "integer", nullable: false),
                    CodeSnippet = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Complexity = table.Column<int>(type: "integer", nullable: false),
                    Dependencies = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTStatements_ASTFileAnalyses_FileAnalysisId",
                        column: x => x.FileAnalysisId,
                        principalTable: "ASTFileAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClassId = table.Column<int>(type: "integer", nullable: true),
                    FileAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Signature = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StartLine = table.Column<int>(type: "integer", nullable: false),
                    EndLine = table.Column<int>(type: "integer", nullable: false),
                    AccessModifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsStatic = table.Column<bool>(type: "boolean", nullable: false),
                    IsAsync = table.Column<bool>(type: "boolean", nullable: false),
                    ReturnType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CyclomaticComplexity = table.Column<int>(type: "integer", nullable: false),
                    LinesOfCode = table.Column<int>(type: "integer", nullable: false),
                    CalledMethods = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTMethods_ASTClasses_ClassId",
                        column: x => x.ClassId,
                        principalTable: "ASTClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ASTMethods_ASTFileAnalyses_FileAnalysisId",
                        column: x => x.FileAnalysisId,
                        principalTable: "ASTFileAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AccessModifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HasGetter = table.Column<bool>(type: "boolean", nullable: false),
                    HasSetter = table.Column<bool>(type: "boolean", nullable: false),
                    IsStatic = table.Column<bool>(type: "boolean", nullable: false),
                    Line = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTProperties_ASTClasses_ClassId",
                        column: x => x.ClassId,
                        principalTable: "ASTClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASTIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    StatementId = table.Column<int>(type: "integer", nullable: true),
                    MethodId = table.Column<int>(type: "integer", nullable: true),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssueType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Recommendation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Line = table.Column<int>(type: "integer", nullable: false),
                    Column = table.Column<int>(type: "integer", nullable: true),
                    RuleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MoreInfoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTIssues_ASTFileAnalyses_FileAnalysisId",
                        column: x => x.FileAnalysisId,
                        principalTable: "ASTFileAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ASTIssues_ASTMethods_MethodId",
                        column: x => x.MethodId,
                        principalTable: "ASTMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ASTIssues_ASTStatements_StatementId",
                        column: x => x.StatementId,
                        principalTable: "ASTStatements",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ASTParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MethodId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASTParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASTParameters_ASTMethods_MethodId",
                        column: x => x.MethodId,
                        principalTable: "ASTMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASTClasses_FileAnalysisId",
                table: "ASTClasses",
                column: "FileAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTClasses_IsAbstract",
                table: "ASTClasses",
                column: "IsAbstract");

            migrationBuilder.CreateIndex(
                name: "IX_ASTClasses_IsInterface",
                table: "ASTClasses",
                column: "IsInterface");

            migrationBuilder.CreateIndex(
                name: "IX_ASTClasses_Name",
                table: "ASTClasses",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ASTDependencies_DependencyType",
                table: "ASTDependencies",
                column: "DependencyType");

            migrationBuilder.CreateIndex(
                name: "IX_ASTDependencies_IsCircular",
                table: "ASTDependencies",
                column: "IsCircular");

            migrationBuilder.CreateIndex(
                name: "IX_ASTDependencies_RepositoryAnalysisId",
                table: "ASTDependencies",
                column: "RepositoryAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTDependencies_SourceFile",
                table: "ASTDependencies",
                column: "SourceFile");

            migrationBuilder.CreateIndex(
                name: "IX_ASTDependencies_TargetFile",
                table: "ASTDependencies",
                column: "TargetFile");

            migrationBuilder.CreateIndex(
                name: "IX_ASTExports_FileAnalysisId",
                table: "ASTExports",
                column: "FileAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTExports_IsDefault",
                table: "ASTExports",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_ASTExports_Name",
                table: "ASTExports",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ASTExports_Type",
                table: "ASTExports",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ASTFileAnalyses_FilePath",
                table: "ASTFileAnalyses",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_ASTFileAnalyses_IsSupported",
                table: "ASTFileAnalyses",
                column: "IsSupported");

            migrationBuilder.CreateIndex(
                name: "IX_ASTFileAnalyses_Language",
                table: "ASTFileAnalyses",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_ASTFileAnalyses_RepositoryAnalysisId",
                table: "ASTFileAnalyses",
                column: "RepositoryAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTFileMetrics_FileAnalysisId",
                table: "ASTFileMetrics",
                column: "FileAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ASTImports_FileAnalysisId",
                table: "ASTImports",
                column: "FileAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTImports_IsDefaultImport",
                table: "ASTImports",
                column: "IsDefaultImport");

            migrationBuilder.CreateIndex(
                name: "IX_ASTImports_IsNamespaceImport",
                table: "ASTImports",
                column: "IsNamespaceImport");

            migrationBuilder.CreateIndex(
                name: "IX_ASTImports_Module",
                table: "ASTImports",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_ASTIssues_Category",
                table: "ASTIssues",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ASTIssues_FileAnalysisId",
                table: "ASTIssues",
                column: "FileAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTIssues_MethodId",
                table: "ASTIssues",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTIssues_RuleId",
                table: "ASTIssues",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTIssues_Severity",
                table: "ASTIssues",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ASTIssues_StatementId",
                table: "ASTIssues",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTMethods_ClassId",
                table: "ASTMethods",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTMethods_CyclomaticComplexity",
                table: "ASTMethods",
                column: "CyclomaticComplexity");

            migrationBuilder.CreateIndex(
                name: "IX_ASTMethods_FileAnalysisId",
                table: "ASTMethods",
                column: "FileAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTMethods_IsAsync",
                table: "ASTMethods",
                column: "IsAsync");

            migrationBuilder.CreateIndex(
                name: "IX_ASTMethods_IsStatic",
                table: "ASTMethods",
                column: "IsStatic");

            migrationBuilder.CreateIndex(
                name: "IX_ASTMethods_Name",
                table: "ASTMethods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ASTMetrics_RepositoryAnalysisId",
                table: "ASTMetrics",
                column: "RepositoryAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ASTParameters_MethodId",
                table: "ASTParameters",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTParameters_Position",
                table: "ASTParameters",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_ASTProperties_ClassId",
                table: "ASTProperties",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTProperties_IsStatic",
                table: "ASTProperties",
                column: "IsStatic");

            migrationBuilder.CreateIndex(
                name: "IX_ASTProperties_Name",
                table: "ASTProperties",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ASTRepositoryAnalyses_AnalyzedAt",
                table: "ASTRepositoryAnalyses",
                column: "AnalyzedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ASTRepositoryAnalyses_RepositoryId",
                table: "ASTRepositoryAnalyses",
                column: "RepositoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ASTStatements_FileAnalysisId",
                table: "ASTStatements",
                column: "FileAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ASTStatements_Line",
                table: "ASTStatements",
                column: "Line");

            migrationBuilder.CreateIndex(
                name: "IX_ASTStatements_Type",
                table: "ASTStatements",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCodeBlocks_DuplicateType",
                table: "DuplicateCodeBlocks",
                column: "DuplicateType");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCodeBlocks_GroupId",
                table: "DuplicateCodeBlocks",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCodeBlocks_RepositoryAnalysisId",
                table: "DuplicateCodeBlocks",
                column: "RepositoryAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCodeBlocks_SimilarityScore",
                table: "DuplicateCodeBlocks",
                column: "SimilarityScore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASTDependencies");

            migrationBuilder.DropTable(
                name: "ASTExports");

            migrationBuilder.DropTable(
                name: "ASTFileMetrics");

            migrationBuilder.DropTable(
                name: "ASTImports");

            migrationBuilder.DropTable(
                name: "ASTIssues");

            migrationBuilder.DropTable(
                name: "ASTMetrics");

            migrationBuilder.DropTable(
                name: "ASTParameters");

            migrationBuilder.DropTable(
                name: "ASTProperties");

            migrationBuilder.DropTable(
                name: "DuplicateCodeBlocks");

            migrationBuilder.DropTable(
                name: "ASTStatements");

            migrationBuilder.DropTable(
                name: "ASTMethods");

            migrationBuilder.DropTable(
                name: "ASTClasses");

            migrationBuilder.DropTable(
                name: "ASTFileAnalyses");

            migrationBuilder.DropTable(
                name: "ASTRepositoryAnalyses");
        }
    }
}
