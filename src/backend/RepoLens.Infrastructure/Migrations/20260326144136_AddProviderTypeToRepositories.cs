using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepoLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderTypeToRepositories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthTokenReference",
                table: "Repositories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderType",
                table: "Repositories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthTokenReference",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "ProviderType",
                table: "Repositories");
        }
    }
}
