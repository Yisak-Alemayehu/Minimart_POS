using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite;

/// <summary>Shared connection/transaction handed to every repository in a unit of work.</summary>
internal sealed class SqliteSession
{
    public SqliteSession(DbConnection connection) => Connection = connection;

    public DbConnection Connection { get; }
    public DbTransaction? Transaction { get; set; }

    public SqliteCommand Command(string sql)
    {
        if (Connection is not SqliteConnection sqlite)
            throw new InvalidOperationException("AbayPOS.Data currently requires a SQLite connection.");

        var command = sqlite.CreateCommand();
        command.CommandText = sql;
        if (Transaction is not null)
            command.Transaction = (SqliteTransaction)Transaction;
        return command;
    }

    public static void Add(SqliteCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    public static async Task ApplyPragmasAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var foreignKeys = connection.CreateCommand();
        foreignKeys.CommandText = "PRAGMA foreign_keys = ON;";
        await foreignKeys.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        await using var wal = connection.CreateCommand();
        wal.CommandText = "PRAGMA journal_mode = WAL;";
        await wal.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
