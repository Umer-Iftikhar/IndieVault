using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndieVault.Migrations
{
    /// <inheritdoc />
    public partial class AddedRawgSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalApiId",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalApiSource",
                table: "Games",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsFromExternalApi",
                table: "Games",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalApiId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ExternalApiSource",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsFromExternalApi",
                table: "Games");
        }
    }
}
