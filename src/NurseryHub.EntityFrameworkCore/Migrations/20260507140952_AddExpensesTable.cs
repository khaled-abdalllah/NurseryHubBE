using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddExpensesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppExpenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpenseNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpenseCategory = table.Column<int>(type: "int", nullable: false),
                    CustomExpenseCategory = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReceiptFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_AppExpenses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenses_TenantId_ExpenseCategory",
                table: "AppExpenses",
                columns: new[] { "TenantId", "ExpenseCategory" });

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenses_TenantId_ExpenseDate",
                table: "AppExpenses",
                columns: new[] { "TenantId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenses_TenantId_ExpenseNumber",
                table: "AppExpenses",
                columns: new[] { "TenantId", "ExpenseNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenses_TenantId_PaymentMethod",
                table: "AppExpenses",
                columns: new[] { "TenantId", "PaymentMethod" });

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenses_TenantId_PaymentStatus",
                table: "AppExpenses",
                columns: new[] { "TenantId", "PaymentStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExpenses");
        }
    }
}
