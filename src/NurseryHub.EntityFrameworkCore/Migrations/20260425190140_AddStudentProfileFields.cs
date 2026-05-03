using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AttendsFriday",
                table: "AppStudents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendsMonday",
                table: "AppStudents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendsThursday",
                table: "AppStudents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendsTuesday",
                table: "AppStudents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendsWednesday",
                table: "AppStudents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DietaryRestrictions",
                table: "AppStudents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthNotes",
                table: "AppStudents",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeAddress",
                table: "AppStudents",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Religion",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToiletTrainingStatus",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendsFriday",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "AttendsMonday",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "AttendsThursday",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "AttendsTuesday",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "AttendsWednesday",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "DietaryRestrictions",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "HealthNotes",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "HomeAddress",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Religion",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "ToiletTrainingStatus",
                table: "AppStudents");
        }
    }
}
