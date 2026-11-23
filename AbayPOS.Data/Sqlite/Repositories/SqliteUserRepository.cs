using AbayPOS.Data.Repositories;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite.Repositories;

internal sealed class SqliteUserRepository : IUserRepository
{
    private readonly SqliteSession _session;

    public SqliteUserRepository(SqliteSession session) => _session = session;

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT * FROM Users WHERE Id = @id;");
        SqliteSession.Add(cmd, "@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? Map(reader) : null;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT * FROM Users WHERE Username = @u;");
        SqliteSession.Add(cmd, "@u", username);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT * FROM Users ORDER BY FullName;");
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<User>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(Map(reader));
        return list;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT COUNT(*) FROM Users;");
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result, null);
    }

    public async Task<int> InsertAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO Users (Username, FullName, PasswordHash, Salt, Role, FailedAttempts, IsLocked, IsActive, CreatedAt)
            VALUES (@Username, @FullName, @PasswordHash, @Salt, @Role, @FailedAttempts, @IsLocked, @IsActive, @CreatedAt);
            SELECT last_insert_rowid();
            """);
        Bind(cmd, user);
        var id = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        user.Id = Convert.ToInt32(id, null);
        return user.Id;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            UPDATE Users SET
                Username = @Username, FullName = @FullName, PasswordHash = @PasswordHash, Salt = @Salt,
                Role = @Role, FailedAttempts = @FailedAttempts, IsLocked = @IsLocked, IsActive = @IsActive
            WHERE Id = @Id;
            """);
        Bind(cmd, user);
        SqliteSession.Add(cmd, "@Id", user.Id);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateLockoutAsync(int userId, int failedAttempts, bool isLocked, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            UPDATE Users SET FailedAttempts = @a, IsLocked = @l WHERE Id = @id;
            """);
        SqliteSession.Add(cmd, "@a", failedAttempts);
        SqliteSession.Add(cmd, "@l", isLocked ? 1 : 0);
        SqliteSession.Add(cmd, "@id", userId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void Bind(SqliteCommand cmd, User user)
    {
        SqliteSession.Add(cmd, "@Username", user.Username);
        SqliteSession.Add(cmd, "@FullName", user.FullName);
        SqliteSession.Add(cmd, "@PasswordHash", user.PasswordHash);
        SqliteSession.Add(cmd, "@Salt", user.Salt);
        SqliteSession.Add(cmd, "@Role", user.Role.ToString());
        SqliteSession.Add(cmd, "@FailedAttempts", user.FailedAttempts);
        SqliteSession.Add(cmd, "@IsLocked", user.IsLocked ? 1 : 0);
        SqliteSession.Add(cmd, "@IsActive", user.IsActive ? 1 : 0);
        SqliteSession.Add(cmd, "@CreatedAt", SqliteHelpers.WriteDate(user.CreatedAt));
    }

    private static User Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Username = reader.GetString(reader.GetOrdinal("Username")),
        FullName = reader.GetString(reader.GetOrdinal("FullName")),
        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
        Salt = reader.GetString(reader.GetOrdinal("Salt")),
        Role = Enum.Parse<UserRole>(reader.GetString(reader.GetOrdinal("Role"))),
        FailedAttempts = reader.GetInt32(reader.GetOrdinal("FailedAttempts")),
        IsLocked = SqliteHelpers.ReadBool(reader, "IsLocked"),
        IsActive = SqliteHelpers.ReadBool(reader, "IsActive"),
        CreatedAt = SqliteHelpers.ReadDate(reader, "CreatedAt")
    };
}
