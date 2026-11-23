using AbayPOS.Data.Repositories;
using AbayPOS.Domain;
using AbayPOS.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace AbayPOS.Data.Sqlite.Repositories;

internal sealed class SqliteProductRepository : IProductRepository
{
    private readonly SqliteSession _session;

    public SqliteProductRepository(SqliteSession session) => _session = session;

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        QueryOneAsync("SELECT * FROM Products WHERE Id = @id;", "@id", id, cancellationToken);

    public Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default) =>
        QueryOneAsync("SELECT * FROM Products WHERE Barcode = @b;", "@b", barcode, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT * FROM Products WHERE IsActive = 1 ORDER BY Name;");
        return await ReadListAsync(cmd, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string? query, string? category, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT * FROM Products WHERE IsActive = 1";
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            sql += " AND Category = @cat";
        if (!string.IsNullOrWhiteSpace(query))
            sql += " AND (Name LIKE @q OR Barcode LIKE @q)";
        sql += " ORDER BY Name;";
        await using var cmd = _session.Command(sql);
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            SqliteSession.Add(cmd, "@cat", category);
        if (!string.IsNullOrWhiteSpace(query))
            SqliteSession.Add(cmd, "@q", "%" + query.Trim() + "%");
        return await ReadListAsync(cmd, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT DISTINCT Category FROM Products WHERE IsActive = 1 ORDER BY Category;");
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<string>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(reader.GetString(0));
        return list;
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT * FROM Products
            WHERE IsActive = 1 AND StockQty <= ReorderLevel
            ORDER BY (StockQty * 1.0) / CASE WHEN ReorderLevel = 0 THEN 1 ELSE ReorderLevel END, Name;
            """);
        return await ReadListAsync(cmd, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, int? exceptId = null, CancellationToken cancellationToken = default)
    {
        var sql = exceptId is null
            ? "SELECT 1 FROM Products WHERE Barcode = @b LIMIT 1;"
            : "SELECT 1 FROM Products WHERE Barcode = @b AND Id <> @id LIMIT 1;";
        await using var cmd = _session.Command(sql);
        SqliteSession.Add(cmd, "@b", barcode);
        if (exceptId is not null)
            SqliteSession.Add(cmd, "@id", exceptId.Value);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is not null && result is not DBNull;
    }

    public async Task<int> InsertAsync(Product product, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            INSERT INTO Products (Barcode, Name, Category, PriceCents, CostPriceCents, StockQty, ReorderLevel, SupplierId, IsActive, Emoji, ImagePath)
            VALUES (@Barcode, @Name, @Category, @PriceCents, @CostPriceCents, @StockQty, @ReorderLevel, @SupplierId, @IsActive, @Emoji, @ImagePath);
            SELECT last_insert_rowid();
            """);
        Bind(cmd, product);
        var id = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        product.Id = Convert.ToInt32(id, null);
        return product.Id;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            UPDATE Products SET
                Barcode = @Barcode, Name = @Name, Category = @Category,
                PriceCents = @PriceCents, CostPriceCents = @CostPriceCents,
                StockQty = @StockQty, ReorderLevel = @ReorderLevel, SupplierId = @SupplierId,
                IsActive = @IsActive, Emoji = @Emoji, ImagePath = @ImagePath
            WHERE Id = @Id;
            """);
        Bind(cmd, product);
        SqliteSession.Add(cmd, "@Id", product.Id);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("UPDATE Products SET IsActive = 0 WHERE Id = @id;");
        SqliteSession.Add(cmd, "@id", id);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> HasSalesAsync(int productId, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("SELECT 1 FROM SaleItems WHERE ProductId = @id LIMIT 1;");
        SqliteSession.Add(cmd, "@id", productId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is not null && result is not DBNull;
    }

    public async Task<int> TryDecrementStockAsync(int productId, int qty, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            UPDATE Products
            SET StockQty = StockQty - @q
            WHERE Id = @id AND StockQty >= @q;
            """);
        SqliteSession.Add(cmd, "@q", qty);
        SqliteSession.Add(cmd, "@id", productId);
        return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task IncrementStockAsync(int productId, int qty, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("UPDATE Products SET StockQty = StockQty + @q WHERE Id = @id;");
        SqliteSession.Add(cmd, "@q", qty);
        SqliteSession.Add(cmd, "@id", productId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<Product?> QueryOneAsync(string sql, string name, object value, CancellationToken cancellationToken)
    {
        await using var cmd = _session.Command(sql);
        SqliteSession.Add(cmd, name, value);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? Map(reader) : null;
    }

    private static async Task<IReadOnlyList<Product>> ReadListAsync(SqliteCommand cmd, CancellationToken cancellationToken)
    {
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<Product>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(Map(reader));
        return list;
    }

    private static void Bind(SqliteCommand cmd, Product product)
    {
        SqliteSession.Add(cmd, "@Barcode", product.Barcode);
        SqliteSession.Add(cmd, "@Name", product.Name);
        SqliteSession.Add(cmd, "@Category", product.Category);
        SqliteSession.Add(cmd, "@PriceCents", Money.ToCents(product.Price));
        SqliteSession.Add(cmd, "@CostPriceCents", Money.ToCents(product.CostPrice));
        SqliteSession.Add(cmd, "@StockQty", product.StockQty);
        SqliteSession.Add(cmd, "@ReorderLevel", product.ReorderLevel);
        SqliteSession.Add(cmd, "@SupplierId", product.SupplierId is null ? DBNull.Value : product.SupplierId);
        SqliteSession.Add(cmd, "@IsActive", product.IsActive ? 1 : 0);
        SqliteSession.Add(cmd, "@Emoji", product.Emoji);
        SqliteSession.Add(cmd, "@ImagePath", product.ImagePath);
    }

    private static Product Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Barcode = reader.GetString(reader.GetOrdinal("Barcode")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Category = reader.GetString(reader.GetOrdinal("Category")),
        Price = SqliteHelpers.ReadMoney(reader, "PriceCents"),
        CostPrice = SqliteHelpers.ReadMoney(reader, "CostPriceCents"),
        StockQty = reader.GetInt32(reader.GetOrdinal("StockQty")),
        ReorderLevel = reader.GetInt32(reader.GetOrdinal("ReorderLevel")),
        SupplierId = SqliteHelpers.ReadNullableInt(reader, "SupplierId"),
        IsActive = SqliteHelpers.ReadBool(reader, "IsActive"),
        Emoji = SqliteHelpers.ReadNullableString(reader, "Emoji"),
        ImagePath = SqliteHelpers.ReadNullableString(reader, "ImagePath")
    };
}
