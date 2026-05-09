using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStudentApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NurseryBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedGradeCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChildFullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ParentFullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ParentPhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SecondaryPhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppStudentApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentApplications_AppGradeCategories_RequestedGradeCategoryId",
                        column: x => x.RequestedGradeCategoryId,
                        principalTable: "AppGradeCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppStudentApplications_AppNurseryBranches_NurseryBranchId",
                        column: x => x.NurseryBranchId,
                        principalTable: "AppNurseryBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentApplications_BirthDate",
                table: "AppStudentApplications",
                column: "BirthDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentApplications_NurseryBranchId",
                table: "AppStudentApplications",
                column: "NurseryBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentApplications_ParentPhoneNumber",
                table: "AppStudentApplications",
                column: "ParentPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentApplications_RequestedGradeCategoryId",
                table: "AppStudentApplications",
                column: "RequestedGradeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentApplications_Status",
                table: "AppStudentApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentApplications_TenantId_CreationTime",
                table: "AppStudentApplications",
                columns: new[] { "TenantId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentApplications_TenantId_NurseryBranchId_Status",
                table: "AppStudentApplications",
                columns: new[] { "TenantId", "NurseryBranchId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStudentApplications");
        }
    }
}
