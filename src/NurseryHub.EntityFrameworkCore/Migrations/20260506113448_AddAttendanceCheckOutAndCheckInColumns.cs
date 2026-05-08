using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceCheckOutAndCheckInColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "AppAttendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedOutAt",
                table: "AppAttendances",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "AppAttendances");

            migrationBuilder.DropColumn(
                name: "CheckedOutAt",
                table: "AppAttendances");
        }
    }
}
