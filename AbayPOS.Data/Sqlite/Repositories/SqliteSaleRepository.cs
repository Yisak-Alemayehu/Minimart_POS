using AbayPOS.Data.Repositories;
using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using AbayPOS.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite.Repositories;

internal sealed class SqliteSaleRepository : ISaleRepository
{
    private readonly SqliteSession _session;

    public SqliteSaleRepository(SqliteSession session) => _session = session;

    public async Task<int> NextSequenceAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT COALESCE(MAX(CAST(SUBSTR(SaleNo, 3) AS INTEGER)), 0) + 1 FROM Sales;
            """);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result, null);
    }

    public async Task<int> InsertAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO Sales (SaleNo, CashierId, SaleDate, SubtotalCents, DiscountCents, TotalCents,
                               PaymentMethod, AmountPaidCents, ChangeCents, Status)
            VALUES (@SaleNo, @CashierId, @SaleDate, @SubtotalCents, @DiscountCents, @TotalCents,
                    @PaymentMethod, @AmountPaidCents, @ChangeCents, @Status);
            SELECT last_insert_rowid();
            """);
        SqliteSession.Add(cmd, "@SaleNo", sale.SaleNo);
        SqliteSession.Add(cmd, "@CashierId", sale.CashierId);
        SqliteSession.Add(cmd, "@SaleDate", SqliteHelpers.WriteDate(sale.SaleDate));
        SqliteSession.Add(cmd, "@SubtotalCents", Money.ToCents(sale.Subtotal));
        SqliteSession.Add(cmd, "@DiscountCents", Money.ToCents(sale.Discount));
        SqliteSession.Add(cmd, "@TotalCents", Money.ToCents(sale.Total));
        SqliteSession.Add(cmd, "@PaymentMethod", sale.PaymentMethod.ToString());
        SqliteSession.Add(cmd, "@AmountPaidCents", Money.ToCents(sale.AmountPaid));
        SqliteSession.Add(cmd, "@ChangeCents", Money.ToCents(sale.Change));
        SqliteSession.Add(cmd, "@Status", sale.Status.ToString());
        var id = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        sale.Id = Convert.ToInt32(id, null);
        return sale.Id;
    }

    public async Task InsertItemAsync(SaleItem item, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO SaleItems (SaleId, ProductId, ProductName, Qty, UnitPriceCents, LineTotalCents)
            VALUES (@SaleId, @ProductId, @ProductName, @Qty, @UnitPriceCents, @LineTotalCents);
            """);
        SqliteSession.Add(cmd, "@SaleId", item.SaleId);
        SqliteSession.Add(cmd, "@ProductId", item.ProductId);
        SqliteSession.Add(cmd, "@ProductName", item.ProductName);
        SqliteSession.Add(cmd, "@Qty", item.Qty);
        SqliteSession.Add(cmd, "@UnitPriceCents", Money.ToCents(item.UnitPrice));
        SqliteSession.Add(cmd, "@LineTotalCents", Money.ToCents(item.LineTotal));
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetOneAsync("WHERE s.Id = @v", id, cancellationToken);

    public Task<Sale?> GetBySaleNoAsync(string saleNo, CancellationToken cancellationToken = default) =>
        GetOneAsync("WHERE s.SaleNo = @v", saleNo, cancellationToken);

    public async Task<IReadOnlyList<Sale>> QueryAsync(DateTime? from, DateTime? to, int? cashierId, string? saleNo, CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT s.*, u.FullName AS CashierName
            FROM Sales s
            JOIN Users u ON u.Id = s.CashierId
            WHERE 1 = 1
            """;
        if (from is not null)
            sql += " AND s.SaleDate >= @from";
        if (to is not null)
            sql += " AND s.SaleDate < @to";
        if (cashierId is not null)
            sql += " AND s.CashierId = @cashier";
        if (!string.IsNullOrWhiteSpace(saleNo))
            sql += " AND s.SaleNo LIKE @saleNo";
        sql += " ORDER BY s.SaleDate DESC;";

        await using var cmd = _session.Command(sql);
        if (from is not null)
            SqliteSession.Add(cmd, "@from", SqliteHelpers.WriteDate(from.Value));
        if (to is not null)
            SqliteSession.Add(cmd, "@to", SqliteHelpers.WriteDate(to.Value));
        if (cashierId is not null)
            SqliteSession.Add(cmd, "@cashier", cashierId.Value);
        if (!string.IsNullOrWhiteSpace(saleNo))
            SqliteSession.Add(cmd, "@saleNo", "%" + saleNo.Trim() + "%");

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<Sale>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(MapSale(reader));
        return list;
    }

    public async Task UpdateStatusAsync(int saleId, SaleStatus status, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("UPDATE Sales SET Status = @s WHERE Id = @id;");
        SqliteSession.Add(cmd, "@s", status.ToString());
        SqliteSession.Add(cmd, "@id", saleId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<Sale?> GetOneAsync(string where, object value, CancellationToken cancellationToken)
    {
        await using var cmd = _session.Command($"""
            SELECT s.*, u.FullName AS CashierName
            FROM Sales s
            JOIN Users u ON u.Id = s.CashierId
            {where};
            """);
        SqliteSession.Add(cmd, "@v", value);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;
        var sale = MapSale(reader);
        sale.Items = await LoadItemsAsync(sale.Id, cancellationToken).ConfigureAwait(false);
        return sale;
    }

    private async Task<IList<SaleItem>> LoadItemsAsync(int saleId, CancellationToken cancellationToken)
    {
        await using var cmd = _session.Command("SELECT * FROM SaleItems WHERE SaleId = @id ORDER BY Id;");
        SqliteSession.Add(cmd, "@id", saleId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var items = new List<SaleItem>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new SaleItem
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SaleId = saleId,
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
                UnitPrice = SqliteHelpers.ReadMoney(reader, "UnitPriceCents"),
                LineTotal = SqliteHelpers.ReadMoney(reader, "LineTotalCents")
            });
        }

        return items;
    }

    private static Sale MapSale(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        SaleNo = reader.GetString(reader.GetOrdinal("SaleNo")),
        CashierId = reader.GetInt32(reader.GetOrdinal("CashierId")),
        CashierName = SqliteHelpers.ReadNullableString(reader, "CashierName"),
        SaleDate = SqliteHelpers.ReadDate(reader, "SaleDate"),
        Subtotal = SqliteHelpers.ReadMoney(reader, "SubtotalCents"),
        Discount = SqliteHelpers.ReadMoney(reader, "DiscountCents"),
        Total = SqliteHelpers.ReadMoney(reader, "TotalCents"),
        PaymentMethod = Enum.Parse<PaymentMethod>(reader.GetString(reader.GetOrdinal("PaymentMethod"))),
        AmountPaid = SqliteHelpers.ReadMoney(reader, "AmountPaidCents"),
        Change = SqliteHelpers.ReadMoney(reader, "ChangeCents"),
        Status = Enum.Parse<SaleStatus>(reader.GetString(reader.GetOrdinal("Status")))
    };
}
