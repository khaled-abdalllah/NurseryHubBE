using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class StoreNurseryLogoInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "AppNurseries");

            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "AppNurseries",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "LogoData",
                table: "AppNurseries",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "AppNurseries");

            migrationBuilder.DropColumn(
                name: "LogoData",
                table: "AppNurseries");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "AppNurseries",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
        }
    }
}
