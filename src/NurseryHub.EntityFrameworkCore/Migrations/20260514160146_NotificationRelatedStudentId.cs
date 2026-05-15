using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class NotificationRelatedStudentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedStudentId",
                table: "AppNotifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppNotifications_RelatedStudentId",
                table: "AppNotifications",
                column: "RelatedStudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppNotifications_AppStudents_RelatedStudentId",
                table: "AppNotifications",
                column: "RelatedStudentId",
                principalTable: "AppStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppNotifications_AppStudents_RelatedStudentId",
                table: "AppNotifications");

            migrationBuilder.DropIndex(
                name: "IX_AppNotifications_RelatedStudentId",
                table: "AppNotifications");

            migrationBuilder.DropColumn(
                name: "RelatedStudentId",
                table: "AppNotifications");
        }
    }
}
