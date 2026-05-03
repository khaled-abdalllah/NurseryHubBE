using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddedGradeCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GradeCategoryId",
                table: "AppNurseryClasses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppGradeCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ColorToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGradeCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryClasses_GradeCategoryId",
                table: "AppNurseryClasses",
                column: "GradeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryClasses_TenantId_GradeCategoryId",
                table: "AppNurseryClasses",
                columns: new[] { "TenantId", "GradeCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGradeCategories_TenantId_Name",
                table: "AppGradeCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AppNurseryClasses_AppGradeCategories_GradeCategoryId",
                table: "AppNurseryClasses",
                column: "GradeCategoryId",
                principalTable: "AppGradeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppNurseryClasses_AppGradeCategories_GradeCategoryId",
                table: "AppNurseryClasses");

            migrationBuilder.DropTable(
                name: "AppGradeCategories");

            migrationBuilder.DropIndex(
                name: "IX_AppNurseryClasses_GradeCategoryId",
                table: "AppNurseryClasses");

            migrationBuilder.DropIndex(
                name: "IX_AppNurseryClasses_TenantId_GradeCategoryId",
                table: "AppNurseryClasses");

            migrationBuilder.DropColumn(
                name: "GradeCategoryId",
                table: "AppNurseryClasses");
        }
    }
}
