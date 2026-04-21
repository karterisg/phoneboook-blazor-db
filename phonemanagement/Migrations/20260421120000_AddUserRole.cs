using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using phonemanagement.Data;

#nullable disable

namespace phonemanagement.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260421120000_AddUserRole")]
public partial class AddUserRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'Users', N'Role') IS NULL
BEGIN
    ALTER TABLE [Users] ADD [Role] nvarchar(50) NOT NULL DEFAULT N'User';
END
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'Users', N'Role') IS NOT NULL
BEGIN
    ALTER TABLE [Users] DROP COLUMN [Role];
END
""");
    }
}

