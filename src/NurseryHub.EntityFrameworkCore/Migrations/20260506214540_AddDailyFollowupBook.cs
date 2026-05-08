using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyFollowupBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppDailyFollowupBooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NurseryBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OverallMood = table.Column<int>(type: "int", nullable: false),
                    TeacherNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SleptToday = table.Column<bool>(type: "bit", nullable: false),
                    SleepDuration = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MoodAfterWaking = table.Column<int>(type: "int", nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SentToParent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SentToParentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AppDailyFollowupBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDailyFollowupBooks_AppNurseryBranches_NurseryBranchId",
                        column: x => x.NurseryBranchId,
                        principalTable: "AppNurseryBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppDailyFollowupBooks_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppDailyFollowupActivityEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DailyFollowupBookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDailyFollowupActivityEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDailyFollowupActivityEntries_AppDailyFollowupBooks_DailyFollowupBookId",
                        column: x => x.DailyFollowupBookId,
                        principalTable: "AppDailyFollowupBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppDailyFollowupMealEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DailyFollowupBookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MealType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDailyFollowupMealEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDailyFollowupMealEntries_AppDailyFollowupBooks_DailyFollowupBookId",
                        column: x => x.DailyFollowupBookId,
                        principalTable: "AppDailyFollowupBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppDailyFollowupSubjectEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DailyFollowupBookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SubjectIcon = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDailyFollowupSubjectEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppDailyFollowupSubjectEntries_AppDailyFollowupBooks_DailyFollowupBookId",
                        column: x => x.DailyFollowupBookId,
                        principalTable: "AppDailyFollowupBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDailyFollowupActivityEntries_DailyFollowupBookId_ActivityType",
                table: "AppDailyFollowupActivityEntries",
                columns: new[] { "DailyFollowupBookId", "ActivityType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDailyFollowupBooks_NurseryBranchId",
                table: "AppDailyFollowupBooks",
                column: "NurseryBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDailyFollowupBooks_StudentId_ReportDate",
                table: "AppDailyFollowupBooks",
                columns: new[] { "StudentId", "ReportDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDailyFollowupBooks_TenantId_NurseryBranchId_ReportDate",
                table: "AppDailyFollowupBooks",
                columns: new[] { "TenantId", "NurseryBranchId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDailyFollowupMealEntries_DailyFollowupBookId_MealType",
                table: "AppDailyFollowupMealEntries",
                columns: new[] { "DailyFollowupBookId", "MealType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDailyFollowupSubjectEntries_DailyFollowupBookId_SortOrder",
                table: "AppDailyFollowupSubjectEntries",
                columns: new[] { "DailyFollowupBookId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDailyFollowupActivityEntries");

            migrationBuilder.DropTable(
                name: "AppDailyFollowupMealEntries");

            migrationBuilder.DropTable(
                name: "AppDailyFollowupSubjectEntries");

            migrationBuilder.DropTable(
                name: "AppDailyFollowupBooks");
        }
    }
}
