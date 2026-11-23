using AbayPOS.Data.Repositories;
using AbayPOS.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite.Repositories;

internal sealed class SqliteAuditRepository : IAuditRepository
{
    private readonly SqliteSession _session;

    public SqliteAuditRepository(SqliteSession session) => _session = session;

    public async Task InsertAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO AuditLog (UserId, Action, Details, CreatedAt)
            VALUES (@UserId, @Action, @Details, @CreatedAt);
            """);
        SqliteSession.Add(cmd, "@UserId", log.UserId);
        SqliteSession.Add(cmd, "@Action", log.Action);
        SqliteSession.Add(cmd, "@Details", log.Details);
        SqliteSession.Add(cmd, "@CreatedAt", SqliteHelpers.WriteDate(log.CreatedAt));
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT a.*, u.Username
            FROM AuditLog a
            LEFT JOIN Users u ON u.Id = a.UserId
            ORDER BY a.CreatedAt DESC
            LIMIT @take;
            """);
        SqliteSession.Add(cmd, "@take", take);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<AuditLog>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            list.Add(new AuditLog
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = SqliteHelpers.ReadNullableInt(reader, "UserId"),
                Username = SqliteHelpers.ReadNullableString(reader, "Username"),
                Action = reader.GetString(reader.GetOrdinal("Action")),
                Details = SqliteHelpers.ReadNullableString(reader, "Details"),
                CreatedAt = SqliteHelpers.ReadDate(reader, "CreatedAt")
            });
        }

        return list;
    }
}
