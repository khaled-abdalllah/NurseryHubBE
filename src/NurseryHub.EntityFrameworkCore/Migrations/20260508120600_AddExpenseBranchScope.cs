using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NurseryHub.EntityFrameworkCore;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(NurseryHubDbContext))]
    [Migration("20260508120600_AddExpenseBranchScope")]
    public partial class AddExpenseBranchScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NurseryBranchId",
                table: "AppExpenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE e
                SET NurseryBranchId = b.Id
                FROM AppExpenses e
                CROSS APPLY (
                    SELECT TOP 1 Id
                    FROM AppNurseryBranches b
                    WHERE (b.TenantId = e.TenantId OR (b.TenantId IS NULL AND e.TenantId IS NULL))
                    ORDER BY b.CreationTime, b.Id
                ) b
                WHERE e.NurseryBranchId IS NULL
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "NurseryBranchId",
                table: "AppExpenses",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenses_NurseryBranchId",
                table: "AppExpenses",
                column: "NurseryBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenses_TenantId_NurseryBranchId_ExpenseDate",
                table: "AppExpenses",
                columns: new[] { "TenantId", "NurseryBranchId", "ExpenseDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppExpenses_AppNurseryBranches_NurseryBranchId",
                table: "AppExpenses",
                column: "NurseryBranchId",
                principalTable: "AppNurseryBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppExpenses_AppNurseryBranches_NurseryBranchId",
                table: "AppExpenses");

            migrationBuilder.DropIndex(
                name: "IX_AppExpenses_NurseryBranchId",
                table: "AppExpenses");

            migrationBuilder.DropIndex(
                name: "IX_AppExpenses_TenantId_NurseryBranchId_ExpenseDate",
                table: "AppExpenses");

            migrationBuilder.DropColumn(
                name: "NurseryBranchId",
                table: "AppExpenses");
        }
    }
}
