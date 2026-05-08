using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddedPaymentsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentPurpose = table.Column<int>(type: "int", nullable: false),
                    CustomPaymentPurpose = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_AppPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPayments_AppGradeCategories_GradeId",
                        column: x => x.GradeId,
                        principalTable: "AppGradeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppPayments_AppNurseryClasses_ClassId",
                        column: x => x.ClassId,
                        principalTable: "AppNurseryClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppPayments_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_ClassId",
                table: "AppPayments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_GradeId",
                table: "AppPayments",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_StudentId",
                table: "AppPayments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_TenantId_ClassId",
                table: "AppPayments",
                columns: new[] { "TenantId", "ClassId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_TenantId_GradeId",
                table: "AppPayments",
                columns: new[] { "TenantId", "GradeId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_TenantId_PaymentDate",
                table: "AppPayments",
                columns: new[] { "TenantId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_TenantId_PaymentNumber",
                table: "AppPayments",
                columns: new[] { "TenantId", "PaymentNumber" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppPayments_TenantId_StudentId",
                table: "AppPayments",
                columns: new[] { "TenantId", "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPayments");
        }
    }
}
