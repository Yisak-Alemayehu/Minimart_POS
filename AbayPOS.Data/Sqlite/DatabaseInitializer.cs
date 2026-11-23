using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite;

/// <summary>Creates <c>abaypos.db</c> from the embedded schema and seed scripts on first run.</summary>
public sealed class DatabaseInitializer
{
    private readonly SqliteConnectionFactory _factory;

    public DatabaseInitializer(SqliteConnectionFactory factory) =>
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>Creates the file, applies schema, and seeds catalog rows when the database is empty.</summary>
    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = (SqliteConnection)_factory.Create();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await SqliteSession.ApplyPragmasAsync(connection, cancellationToken).ConfigureAwait(false);

        await ExecuteScriptAsync(connection, ReadEmbedded("schema.sql"), cancellationToken).ConfigureAwait(false);

        await using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Products;";
        var products = Convert.ToInt32(await count.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), null);
        if (products == 0)
            await ExecuteScriptAsync(connection, ReadEmbedded("seed.sql"), cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExecuteScriptAsync(SqliteConnection connection, string script, CancellationToken cancellationToken)
    {
        foreach (var statement in SplitStatements(script))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static IEnumerable<string> SplitStatements(string script)
    {
        var builder = new StringBuilder();
        using var reader = new StringReader(script);
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("--", StringComparison.Ordinal))
                continue;
            builder.AppendLine(line);
        }

        return builder.ToString()
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(statement => statement.Length > 0)
            .Select(statement => statement + ";");
    }

    private static string ReadEmbedded(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded SQL '{fileName}' was not found.");
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Embedded SQL '{fileName}' could not be opened.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
