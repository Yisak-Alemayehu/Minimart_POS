using AbayPOS.Data.Repositories;
using AbayPOS.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite.Repositories;

internal sealed class SqliteSupplierRepository : ISupplierRepository
{
    private readonly SqliteSession _session;

    public SqliteSupplierRepository(SqliteSession session) => _session = session;

    public async Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT * FROM Suppliers WHERE Id = @id;");
        SqliteSession.Add(cmd, "@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var sql = activeOnly
            ? "SELECT * FROM Suppliers WHERE IsActive = 1 ORDER BY Name;"
            : "SELECT * FROM Suppliers ORDER BY Name;";
        await using var cmd = _session.Command(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<Supplier>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(Map(reader));
        return list;
    }

    public async Task<int> InsertAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO Suppliers (Name, Phone, Email, Address, IsActive)
            VALUES (@Name, @Phone, @Email, @Address, @IsActive);
            SELECT last_insert_rowid();
            """);
        Bind(cmd, supplier);
        var id = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        supplier.Id = Convert.ToInt32(id, null);
        return supplier.Id;
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            UPDATE Suppliers SET Name = @Name, Phone = @Phone, Email = @Email, Address = @Address, IsActive = @IsActive
            WHERE Id = @Id;
            """);
        Bind(cmd, supplier);
        SqliteSession.Add(cmd, "@Id", supplier.Id);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void Bind(SqliteCommand cmd, Supplier supplier)
    {
        SqliteSession.Add(cmd, "@Name", supplier.Name);
        SqliteSession.Add(cmd, "@Phone", supplier.Phone);
        SqliteSession.Add(cmd, "@Email", supplier.Email);
        SqliteSession.Add(cmd, "@Address", supplier.Address);
        SqliteSession.Add(cmd, "@IsActive", supplier.IsActive ? 1 : 0);
    }

    private static Supplier Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Phone = SqliteHelpers.ReadNullableString(reader, "Phone"),
        Email = SqliteHelpers.ReadNullableString(reader, "Email"),
        Address = SqliteHelpers.ReadNullableString(reader, "Address"),
        IsActive = SqliteHelpers.ReadBool(reader, "IsActive")
    };
}
