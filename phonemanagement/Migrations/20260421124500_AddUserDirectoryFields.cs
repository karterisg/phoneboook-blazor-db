using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using phonemanagement.Data;

#nullable disable

namespace phonemanagement.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260421124500_AddUserDirectoryFields")]
public partial class AddUserDirectoryFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'Users', N'Name') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [Name] nvarchar(200) NOT NULL DEFAULT N'';
END
IF COL_LENGTH(N'Users', N'Phone') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [Phone] nvarchar(50) NOT NULL DEFAULT N'';
END
IF COL_LENGTH(N'Users', N'Gender') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [Gender] nvarchar(20) NOT NULL DEFAULT N'Male';
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'Users', N'Gender') IS NOT NULL
BEGIN
    ALTER TABLE [Users] DROP COLUMN [Gender];
END
IF COL_LENGTH(N'Users', N'Phone') IS NOT NULL
BEGIN
    ALTER TABLE [Users] DROP COLUMN [Phone];
END
IF COL_LENGTH(N'Users', N'Name') IS NOT NULL
BEGIN
    ALTER TABLE [Users] DROP COLUMN [Name];
END
""");
    }
}

