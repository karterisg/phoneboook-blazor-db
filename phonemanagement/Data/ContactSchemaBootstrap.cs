using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace phonemanagement.Data;

/// <summary>
/// Προσθέτει idempotent τις στήλες που λείπουν στον πίνακα Contacts (ADO.NET μόνο, χωρίς EF queries).
/// </summary>
public static class ContactSchemaBootstrap
{
    public static async Task EnsureExtendedColumnsAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var cs = db.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
            return;

        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(cancellationToken);

        var schema = await ResolveContactsSchemaAsync(conn, cancellationToken);
        if (schema is null)
            return;

        var fq = $"[{schema.Replace("]", "]]")}].[Contacts]";

        await ExecAsync(conn, cancellationToken, $"""
            IF NOT EXISTS (SELECT 1 FROM sys.columns col INNER JOIN sys.tables t ON col.object_id = t.object_id INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = N'Contacts' AND s.name = N'{schema.Replace("'", "''")}' AND col.name = N'IsUserContribution')
            ALTER TABLE {fq} ADD [IsUserContribution] bit NOT NULL CONSTRAINT DF_Contacts_IsUserContribution DEFAULT 0;
            """);

        await ExecAsync(conn, cancellationToken, $"""
            IF NOT EXISTS (SELECT 1 FROM sys.columns col INNER JOIN sys.tables t ON col.object_id = t.object_id INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = N'Contacts' AND s.name = N'{schema.Replace("'", "''")}' AND col.name = N'DirectoryListingId')
            ALTER TABLE {fq} ADD [DirectoryListingId] uniqueidentifier NULL;
            """);

        await ExecAsync(conn, cancellationToken, $"""
            IF NOT EXISTS (SELECT 1 FROM sys.columns col INNER JOIN sys.tables t ON col.object_id = t.object_id INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = N'Contacts' AND s.name = N'{schema.Replace("'", "''")}' AND col.name = N'CreatedAtUtc')
            ALTER TABLE {fq} ADD [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT DF_Contacts_CreatedAtUtc DEFAULT SYSUTCDATETIME();
            """);

        await ExecAsync(conn, cancellationToken, $"""
            IF EXISTS (SELECT 1 FROM sys.columns col INNER JOIN sys.tables t ON col.object_id = t.object_id INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = N'Contacts' AND s.name = N'{schema.Replace("'", "''")}' AND col.name = N'DirectoryListingId')
            AND NOT EXISTS (SELECT 1 FROM sys.indexes i WHERE i.object_id = OBJECT_ID(N'{schema.Replace("'", "''")}.Contacts') AND i.name = N'IX_Contacts_DirectoryListingId')
            CREATE UNIQUE NONCLUSTERED INDEX [IX_Contacts_DirectoryListingId] ON {fq}([DirectoryListingId]) WHERE [DirectoryListingId] IS NOT NULL;
            """);
    }

    static async Task<string?> ResolveContactsSchemaAsync(SqlConnection conn, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(
            """
            SELECT TOP (1) SCHEMA_NAME(t.schema_id)
            FROM sys.tables AS t
            WHERE t.name = N'Contacts'
            ORDER BY SCHEMA_NAME(t.schema_id);
            """,
            conn);
        var o = await cmd.ExecuteScalarAsync(ct);
        return o is string s ? s : null;
    }

    static async Task ExecAsync(SqlConnection conn, CancellationToken ct, string sql)
    {
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
