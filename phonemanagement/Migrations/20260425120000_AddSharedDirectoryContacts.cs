using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using phonemanagement.Data;

#nullable disable

namespace phonemanagement.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260425120000_AddSharedDirectoryContacts")]
public partial class AddSharedDirectoryContacts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'Contacts', N'IsUserContribution') IS NULL
BEGIN
    ALTER TABLE [Contacts] ADD [IsUserContribution] bit NOT NULL CONSTRAINT DF_Contacts_IsUserContribution DEFAULT 0;
END
IF COL_LENGTH(N'Contacts', N'DirectoryListingId') IS NULL
BEGIN
    ALTER TABLE [Contacts] ADD [DirectoryListingId] uniqueidentifier NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Contacts_DirectoryListingId' AND object_id = OBJECT_ID(N'Contacts'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Contacts_DirectoryListingId] ON [Contacts]([DirectoryListingId]) WHERE [DirectoryListingId] IS NOT NULL;
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Contacts_DirectoryListingId' AND object_id = OBJECT_ID(N'Contacts'))
BEGIN
    DROP INDEX [IX_Contacts_DirectoryListingId] ON [Contacts];
END
IF COL_LENGTH(N'Contacts', N'DirectoryListingId') IS NOT NULL
BEGIN
    ALTER TABLE [Contacts] DROP COLUMN [DirectoryListingId];
END
IF COL_LENGTH(N'Contacts', N'IsUserContribution') IS NOT NULL
BEGIN
    ALTER TABLE [Contacts] DROP COLUMN [IsUserContribution];
END
""");
    }
}
