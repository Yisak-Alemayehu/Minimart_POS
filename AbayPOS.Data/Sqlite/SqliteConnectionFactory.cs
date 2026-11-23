using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite;

/// <summary>Builds SQLite connections. Replace this class with a MySQL factory to switch engines.</summary>
public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(databasePath)) ?? ".");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true
        }.ToString();
        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    /// <inheritdoc />
    public DbConnection Create() => new SqliteConnection(_connectionString);
}
