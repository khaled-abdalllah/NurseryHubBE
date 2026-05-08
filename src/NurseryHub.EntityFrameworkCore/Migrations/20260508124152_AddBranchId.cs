using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NurseryBranchId",
                table: "AppGradeCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE gc
                SET NurseryBranchId = x.NurseryBranchId
                FROM AppGradeCategories gc
                CROSS APPLY (
                    SELECT TOP 1 nc.NurseryBranchId
                    FROM AppNurseryClasses nc
                    WHERE nc.GradeCategoryId = gc.Id
                    ORDER BY nc.CreationTime
                ) x
                WHERE gc.NurseryBranchId IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE gc
                SET NurseryBranchId = x.Id
                FROM AppGradeCategories gc
                CROSS APPLY (
                    SELECT TOP 1 nb.Id
                    FROM AppNurseryBranches nb
                    WHERE nb.TenantId = gc.TenantId
                    ORDER BY nb.CreationTime
                ) x
                WHERE gc.NurseryBranchId IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE gc
                SET NurseryBranchId = x.Id
                FROM AppGradeCategories gc
                CROSS APPLY (
                    SELECT TOP 1 nb.Id
                    FROM AppNurseryBranches nb
                    ORDER BY nb.CreationTime
                ) x
                WHERE gc.NurseryBranchId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "NurseryBranchId",
                table: "AppGradeCategories",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_AppGradeCategories_TenantId_Name",
                table: "AppGradeCategories");

            migrationBuilder.CreateIndex(
                name: "IX_AppGradeCategories_NurseryBranchId",
                table: "AppGradeCategories",
                column: "NurseryBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGradeCategories_TenantId_NurseryBranchId_Name",
                table: "AppGradeCategories",
                columns: new[] { "TenantId", "NurseryBranchId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AppGradeCategories_AppNurseryBranches_NurseryBranchId",
                table: "AppGradeCategories",
                column: "NurseryBranchId",
                principalTable: "AppNurseryBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppGradeCategories_AppNurseryBranches_NurseryBranchId",
                table: "AppGradeCategories");

            migrationBuilder.DropIndex(
                name: "IX_AppGradeCategories_NurseryBranchId",
                table: "AppGradeCategories");

            migrationBuilder.DropIndex(
                name: "IX_AppGradeCategories_TenantId_NurseryBranchId_Name",
                table: "AppGradeCategories");

            migrationBuilder.CreateIndex(
                name: "IX_AppGradeCategories_TenantId_Name",
                table: "AppGradeCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "NurseryBranchId",
                table: "AppGradeCategories");
        }
    }
}
