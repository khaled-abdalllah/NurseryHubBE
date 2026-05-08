using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NurseryHub.Migrations
{
    /// <inheritdoc />
    public partial class FixGradeCategoryBranchScopeRepair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('AppGradeCategories', 'NurseryBranchId') IS NULL
                BEGIN
                    ALTER TABLE [AppGradeCategories] ADD [NurseryBranchId] uniqueidentifier NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                UPDATE gc
                SET NurseryBranchId = x.NurseryBranchId
                FROM AppGradeCategories gc
                CROSS APPLY (
                    SELECT TOP 1 nc.NurseryBranchId
                    FROM AppNurseryClasses nc
                    WHERE nc.GradeCategoryId = gc.Id
                    ORDER BY nc.CreationTime
                ) x
                WHERE gc.NurseryBranchId IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE gc
                SET NurseryBranchId = x.Id
                FROM AppGradeCategories gc
                CROSS APPLY (
                    SELECT TOP 1 nb.Id
                    FROM AppNurseryBranches nb
                    WHERE nb.TenantId = gc.TenantId
                    ORDER BY nb.CreationTime
                ) x
                WHERE gc.NurseryBranchId IS NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE gc
                SET NurseryBranchId = x.Id
                FROM AppGradeCategories gc
                CROSS APPLY (
                    SELECT TOP 1 nb.Id
                    FROM AppNurseryBranches nb
                    ORDER BY nb.CreationTime
                ) x
                WHERE gc.NurseryBranchId IS NULL;
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM AppGradeCategories
                    WHERE NurseryBranchId IS NULL
                )
                BEGIN
                    THROW 51000, 'Unable to backfill NurseryBranchId for all grade categories.', 1;
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_AppGradeCategories_TenantId_Name'
                      AND object_id = OBJECT_ID('AppGradeCategories')
                )
                BEGIN
                    DROP INDEX [IX_AppGradeCategories_TenantId_Name] ON [AppGradeCategories];
                END
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE [AppGradeCategories]
                ALTER COLUMN [NurseryBranchId] uniqueidentifier NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_AppGradeCategories_NurseryBranchId'
                      AND object_id = OBJECT_ID('AppGradeCategories')
                )
                BEGIN
                    CREATE INDEX [IX_AppGradeCategories_NurseryBranchId]
                    ON [AppGradeCategories]([NurseryBranchId]);
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_AppGradeCategories_TenantId_NurseryBranchId_Name'
                      AND object_id = OBJECT_ID('AppGradeCategories')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_AppGradeCategories_TenantId_NurseryBranchId_Name]
                    ON [AppGradeCategories]([TenantId], [NurseryBranchId], [Name])
                    WHERE [TenantId] IS NOT NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_AppGradeCategories_AppNurseryBranches_NurseryBranchId'
                )
                BEGIN
                    ALTER TABLE [AppGradeCategories] ADD CONSTRAINT [FK_AppGradeCategories_AppNurseryBranches_NurseryBranchId]
                    FOREIGN KEY ([NurseryBranchId]) REFERENCES [AppNurseryBranches]([Id]) ON DELETE NO ACTION;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = 'FK_AppGradeCategories_AppNurseryBranches_NurseryBranchId'
                )
                BEGIN
                    ALTER TABLE [AppGradeCategories]
                    DROP CONSTRAINT [FK_AppGradeCategories_AppNurseryBranches_NurseryBranchId];
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_AppGradeCategories_NurseryBranchId'
                      AND object_id = OBJECT_ID('AppGradeCategories')
                )
                BEGIN
                    DROP INDEX [IX_AppGradeCategories_NurseryBranchId] ON [AppGradeCategories];
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_AppGradeCategories_TenantId_NurseryBranchId_Name'
                      AND object_id = OBJECT_ID('AppGradeCategories')
                )
                BEGIN
                    DROP INDEX [IX_AppGradeCategories_TenantId_NurseryBranchId_Name] ON [AppGradeCategories];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('AppGradeCategories', 'NurseryBranchId') IS NOT NULL
                BEGIN
                    ALTER TABLE [AppGradeCategories] DROP COLUMN [NurseryBranchId];
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_AppGradeCategories_TenantId_Name'
                      AND object_id = OBJECT_ID('AppGradeCategories')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_AppGradeCategories_TenantId_Name]
                    ON [AppGradeCategories]([TenantId], [Name])
                    WHERE [TenantId] IS NOT NULL;
                END
                """);
        }
    }
}
