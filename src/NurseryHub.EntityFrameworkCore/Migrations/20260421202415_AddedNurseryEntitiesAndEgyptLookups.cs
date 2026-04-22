using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class AddedNurseryEntitiesAndEgyptLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppGovernorates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGovernorates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNurseries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_AppNurseries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GovernorateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCities_AppGovernorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "AppGovernorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppNurseryBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NurseryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsTransportationIncluded = table.Column<bool>(type: "bit", nullable: false),
                    IsMealsIncluded = table.Column<bool>(type: "bit", nullable: false),
                    GovernorateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacebookLink = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_AppNurseryBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppNurseryBranches_AppCities_CityId",
                        column: x => x.CityId,
                        principalTable: "AppCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppNurseryBranches_AppGovernorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "AppGovernorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppNurseryBranches_AppNurseries_NurseryId",
                        column: x => x.NurseryId,
                        principalTable: "AppNurseries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppNurseryClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NurseryBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    MinAgeInMonths = table.Column<int>(type: "int", nullable: true),
                    MaxAgeInMonths = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AppNurseryClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppNurseryClasses_AppNurseryBranches_NurseryBranchId",
                        column: x => x.NurseryBranchId,
                        principalTable: "AppNurseryBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUserBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NurseryBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserBranches_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserBranches_AppNurseryBranches_NurseryBranchId",
                        column: x => x.NurseryBranchId,
                        principalTable: "AppNurseryBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCities_Code",
                table: "AppCities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCities_GovernorateId_NameEn",
                table: "AppCities",
                columns: new[] { "GovernorateId", "NameEn" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGovernorates_Code",
                table: "AppGovernorates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseries_TenantId_Email",
                table: "AppNurseries",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseries_TenantId_Name",
                table: "AppNurseries",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryBranches_CityId",
                table: "AppNurseryBranches",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryBranches_GovernorateId",
                table: "AppNurseryBranches",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryBranches_NurseryId",
                table: "AppNurseryBranches",
                column: "NurseryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryBranches_TenantId_GovernorateId_CityId",
                table: "AppNurseryBranches",
                columns: new[] { "TenantId", "GovernorateId", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryBranches_TenantId_NurseryId_Name",
                table: "AppNurseryBranches",
                columns: new[] { "TenantId", "NurseryId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryClasses_NurseryBranchId",
                table: "AppNurseryClasses",
                column: "NurseryBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNurseryClasses_TenantId_NurseryBranchId_Name",
                table: "AppNurseryClasses",
                columns: new[] { "TenantId", "NurseryBranchId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserBranches_NurseryBranchId",
                table: "AppUserBranches",
                column: "NurseryBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserBranches_TenantId_NurseryBranchId",
                table: "AppUserBranches",
                columns: new[] { "TenantId", "NurseryBranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserBranches_TenantId_UserId_NurseryBranchId",
                table: "AppUserBranches",
                columns: new[] { "TenantId", "UserId", "NurseryBranchId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserBranches_UserId",
                table: "AppUserBranches",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNurseryClasses");

            migrationBuilder.DropTable(
                name: "AppUserBranches");

            migrationBuilder.DropTable(
                name: "AppNurseryBranches");

            migrationBuilder.DropTable(
                name: "AppCities");

            migrationBuilder.DropTable(
                name: "AppNurseries");

            migrationBuilder.DropTable(
                name: "AppGovernorates");
        }
    }
}
