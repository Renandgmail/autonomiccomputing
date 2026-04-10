using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepoLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRepositorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CalculatedAt",
                table: "RepositoryMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ComplexityMetrics",
                table: "RepositoryMetrics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RepositoryMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCommitDate",
                table: "RepositoryMetrics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OverallScore",
                table: "RepositoryMetrics",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "QualityScore",
                table: "RepositoryMetrics",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SecurityScore",
                table: "RepositoryMetrics",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TechnicalDebt",
                table: "RepositoryMetrics",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCommits",
                table: "RepositoryMetrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLines",
                table: "RepositoryMetrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "TotalSize",
                table: "RepositoryMetrics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RepositoryMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "RepositoryFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CyclomaticComplexity",
                table: "RepositoryFiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TestCoveragePercentage",
                table: "RepositoryFiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocal",
                table: "Repositories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocalPath",
                table: "Repositories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Repositories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Repositories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Additions",
                table: "ContributorMetrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "ContributorMetrics",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ContributorMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Deletions",
                table: "ContributorMetrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ContributorMetrics",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FilesChanged",
                table: "ContributorMetrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstCommit",
                table: "ContributorMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryContributor",
                table: "ContributorMetrics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCommit",
                table: "ContributorMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCommitAt",
                table: "ContributorMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ContributorMetrics",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ContributorMetrics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "CodeElements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullContent",
                table: "CodeElements",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalculatedAt",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "ComplexityMetrics",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "LastCommitDate",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "OverallScore",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "QualityScore",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "SecurityScore",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "TechnicalDebt",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "TotalCommits",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "TotalLines",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "TotalSize",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RepositoryMetrics");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "RepositoryFiles");

            migrationBuilder.DropColumn(
                name: "CyclomaticComplexity",
                table: "RepositoryFiles");

            migrationBuilder.DropColumn(
                name: "TestCoveragePercentage",
                table: "RepositoryFiles");

            migrationBuilder.DropColumn(
                name: "IsLocal",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "LocalPath",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "Additions",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "Deletions",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "FilesChanged",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "FirstCommit",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "IsPrimaryContributor",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "LastCommit",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "LastCommitAt",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ContributorMetrics");

            migrationBuilder.DropColumn(
                name: "File",
                table: "CodeElements");

            migrationBuilder.DropColumn(
                name: "FullContent",
                table: "CodeElements");
        }
    }
}
