using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RepoLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeIntelligenceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScanErrorMessage",
                table: "Repositories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScanStatus",
                table: "Repositories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalFiles",
                table: "Repositories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLines",
                table: "Repositories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RepositoryFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProcessingStatus = table.Column<string>(type: "text", nullable: false),
                    ProcessingTime = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepositoryFiles_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodeElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileId = table.Column<int>(type: "integer", nullable: false),
                    ElementType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FullyQualifiedName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartLine = table.Column<int>(type: "integer", nullable: false),
                    EndLine = table.Column<int>(type: "integer", nullable: false),
                    Signature = table.Column<string>(type: "text", nullable: true),
                    AccessModifier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsStatic = table.Column<bool>(type: "boolean", nullable: false),
                    IsAsync = table.Column<bool>(type: "boolean", nullable: false),
                    ReturnType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Parameters = table.Column<string>(type: "TEXT", nullable: true),
                    Documentation = table.Column<string>(type: "text", nullable: true),
                    Complexity = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeElements_RepositoryFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "RepositoryFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeElements_ElementType",
                table: "CodeElements",
                column: "ElementType");

            migrationBuilder.CreateIndex(
                name: "IX_CodeElements_FileId",
                table: "CodeElements",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeElements_Name",
                table: "CodeElements",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryFiles_FileExtension",
                table: "RepositoryFiles",
                column: "FileExtension");

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryFiles_Language",
                table: "RepositoryFiles",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryFiles_ProcessingStatus",
                table: "RepositoryFiles",
                column: "ProcessingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryFiles_RepositoryId_FilePath",
                table: "RepositoryFiles",
                columns: new[] { "RepositoryId", "FilePath" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeElements");

            migrationBuilder.DropTable(
                name: "RepositoryFiles");

            migrationBuilder.DropColumn(
                name: "ScanErrorMessage",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "ScanStatus",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "TotalFiles",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "TotalLines",
                table: "Repositories");
        }
    }
}
