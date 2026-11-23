using AbayPOS.Data.Repositories;
using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite.Repositories;

internal sealed class SqliteStockRepository : IStockRepository
{
    private readonly SqliteSession _session;

    public SqliteStockRepository(SqliteSession session) => _session = session;

    public async Task<int> InsertReceiptAsync(StockReceipt receipt, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO StockReceipts (SupplierId, ReceivedBy, ReceivedAt, Note)
            VALUES (@SupplierId, @ReceivedBy, @ReceivedAt, @Note);
            SELECT last_insert_rowid();
            """);
        SqliteSession.Add(cmd, "@SupplierId", receipt.SupplierId);
        SqliteSession.Add(cmd, "@ReceivedBy", receipt.ReceivedBy);
        SqliteSession.Add(cmd, "@ReceivedAt", SqliteHelpers.WriteDate(receipt.ReceivedAt));
        SqliteSession.Add(cmd, "@Note", receipt.Note);
        var id = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        receipt.Id = Convert.ToInt32(id, null);
        return receipt.Id;
    }

    public async Task InsertReceiptItemAsync(StockReceiptItem item, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO StockReceiptItems (ReceiptId, ProductId, Qty, UnitCostCents)
            VALUES (@ReceiptId, @ProductId, @Qty, @UnitCostCents);
            """);
        SqliteSession.Add(cmd, "@ReceiptId", item.ReceiptId);
        SqliteSession.Add(cmd, "@ProductId", item.ProductId);
        SqliteSession.Add(cmd, "@Qty", item.Qty);
        SqliteSession.Add(cmd, "@UnitCostCents", Money.ToCents(item.UnitCost));
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<StockReceipt>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT r.*, s.Name AS SupplierName, u.FullName AS ReceivedByName
            FROM StockReceipts r
            JOIN Suppliers s ON s.Id = r.SupplierId
            JOIN Users u ON u.Id = r.ReceivedBy
            ORDER BY r.ReceivedAt DESC
            LIMIT @take;
            """);
        SqliteSession.Add(cmd, "@take", take);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<StockReceipt>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            list.Add(new StockReceipt
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SupplierId = reader.GetInt32(reader.GetOrdinal("SupplierId")),
                SupplierName = SqliteHelpers.ReadNullableString(reader, "SupplierName"),
                ReceivedBy = reader.GetInt32(reader.GetOrdinal("ReceivedBy")),
                ReceivedByName = SqliteHelpers.ReadNullableString(reader, "ReceivedByName"),
                ReceivedAt = SqliteHelpers.ReadDate(reader, "ReceivedAt"),
                Note = SqliteHelpers.ReadNullableString(reader, "Note")
            });
        }

        return list;
    }
}
