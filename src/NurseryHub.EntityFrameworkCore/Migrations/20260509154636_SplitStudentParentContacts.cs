using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class SplitStudentParentContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppParentContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FatherIdentityNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FatherPhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MotherName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MotherIdentityNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MotherPhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_AppParentContacts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppParentContacts_TenantId",
                table: "AppParentContacts",
                column: "TenantId");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "AppStudents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                SELECT Id AS StudentId, NEWID() AS NewParentId, TenantId, FatherName, FatherIdentityNumber, FatherPhoneNumber, MotherName, MotherIdentityNumber, MotherPhoneNumber, CreationTime, CreatorId, LastModificationTime, LastModifierId, IsDeleted, DeleterId, DeletionTime
                INTO #ParentMap
                FROM AppStudents;

                INSERT INTO AppParentContacts (Id, TenantId, FatherName, FatherIdentityNumber, FatherPhoneNumber, MotherName, MotherIdentityNumber, MotherPhoneNumber, CreationTime, CreatorId, LastModificationTime, LastModifierId, IsDeleted, DeleterId, DeletionTime)
                SELECT NewParentId, TenantId, FatherName, FatherIdentityNumber, FatherPhoneNumber, MotherName, MotherIdentityNumber, MotherPhoneNumber, CreationTime, CreatorId, LastModificationTime, LastModifierId, IsDeleted, DeleterId, DeletionTime
                FROM #ParentMap;

                UPDATE s
                SET s.ParentId = m.NewParentId
                FROM AppStudents s
                INNER JOIN #ParentMap m ON s.Id = m.StudentId;

                DROP TABLE #ParentMap;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "AppStudents",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_ParentId",
                table: "AppStudents",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStudents_AppParentContacts_ParentId",
                table: "AppStudents",
                column: "ParentId",
                principalTable: "AppParentContacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "FatherIdentityNumber",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "FatherPhoneNumber",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "MotherIdentityNumber",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "MotherName",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "MotherPhoneNumber",
                table: "AppStudents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStudents_AppParentContacts_ParentId",
                table: "AppStudents");

            migrationBuilder.AddColumn<string>(
                name: "FatherIdentityNumber",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "AppStudents",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FatherPhoneNumber",
                table: "AppStudents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotherIdentityNumber",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotherName",
                table: "AppStudents",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotherPhoneNumber",
                table: "AppStudents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE s
                SET FatherName = p.FatherName,
                    FatherIdentityNumber = p.FatherIdentityNumber,
                    FatherPhoneNumber = p.FatherPhoneNumber,
                    MotherName = p.MotherName,
                    MotherIdentityNumber = p.MotherIdentityNumber,
                    MotherPhoneNumber = p.MotherPhoneNumber
                FROM AppStudents s
                INNER JOIN AppParentContacts p ON s.ParentId = p.Id
                WHERE s.ParentId IS NOT NULL
                """);

            migrationBuilder.DropIndex(
                name: "IX_AppStudents_ParentId",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "AppStudents");

            migrationBuilder.DropTable(
                name: "AppParentContacts");
        }
    }
}
