using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddedStudentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NurseryBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NurseryClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FatherIdentityNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FatherPhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MotherName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MotherIdentityNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MotherPhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EmergencyContactNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EnrollmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MedicalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_AppStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudents_AppNurseryBranches_NurseryBranchId",
                        column: x => x.NurseryBranchId,
                        principalTable: "AppNurseryBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppStudents_AppNurseryClasses_NurseryClassId",
                        column: x => x.NurseryClassId,
                        principalTable: "AppNurseryClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_NurseryBranchId",
                table: "AppStudents",
                column: "NurseryBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_NurseryClassId",
                table: "AppStudents",
                column: "NurseryClassId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_TenantId_NurseryBranchId_FullName",
                table: "AppStudents",
                columns: new[] { "TenantId", "NurseryBranchId", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_TenantId_NurseryClassId",
                table: "AppStudents",
                columns: new[] { "TenantId", "NurseryClassId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStudents");
        }
    }
}
