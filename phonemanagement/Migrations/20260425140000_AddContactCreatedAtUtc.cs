using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using phonemanagement.Data;

#nullable disable

namespace phonemanagement.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260425140000_AddContactCreatedAtUtc")]
public partial class AddContactCreatedAtUtc : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'Contacts', N'CreatedAtUtc') IS NULL
BEGIN
    ALTER TABLE [Contacts] ADD [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT DF_Contacts_CreatedAtUtc DEFAULT SYSUTCDATETIME();
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'Contacts', N'CreatedAtUtc') IS NOT NULL
BEGIN
    ALTER TABLE [Contacts] DROP CONSTRAINT DF_Contacts_CreatedAtUtc;
    ALTER TABLE [Contacts] DROP COLUMN [CreatedAtUtc];
END
""");
    }
}
