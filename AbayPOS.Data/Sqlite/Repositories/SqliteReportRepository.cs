using AbayPOS.Data.Repositories;
using AbayPOS.Domain.Reporting;

namespace AbayPOS.Data.Sqlite.Repositories;

internal sealed class SqliteReportRepository : IReportRepository
{
    private readonly SqliteSession _session;

    public SqliteReportRepository(SqliteSession session) => _session = session;

    public async Task<DashboardKpis> GetDashboardAsync(DateTime today, CancellationToken cancellationToken = default)
    {
        var start = today.Date;
        var end = start.AddDays(1);
        await using var sales = _session.Command("""
            SELECT COALESCE(SUM(TotalCents), 0), COUNT(*)
            FROM Sales
            WHERE Status = 'Completed' AND SaleDate >= @from AND SaleDate < @to;
            """);
        SqliteSession.Add(sales, "@from", SqliteHelpers.WriteDate(start));
        SqliteSession.Add(sales, "@to", SqliteHelpers.WriteDate(end));
        await using var reader = await sales.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        var total = AbayPOS.Domain.Money.FromCents(reader.GetInt64(0));
        var count = reader.GetInt32(1);

        await using var low = _session.Command("SELECT COUNT(*) FROM Products WHERE IsActive = 1 AND StockQty <= ReorderLevel;");
        var lowCount = Convert.ToInt32(await low.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), null);

        return new DashboardKpis
        {
            TodaySales = total,
            TodayTransactions = count,
            AverageBasket = count == 0 ? 0m : decimal.Round(total / count, 2, MidpointRounding.AwayFromZero),
            LowStockCount = lowCount
        };
    }

    public async Task<IReadOnlyList<DailySalesPoint>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT date(SaleDate) AS Day, COALESCE(SUM(TotalCents), 0), COUNT(*)
            FROM Sales
            WHERE Status = 'Completed' AND SaleDate >= @from AND SaleDate < @to
            GROUP BY date(SaleDate)
            ORDER BY Day;
            """);
        SqliteSession.Add(cmd, "@from", SqliteHelpers.WriteDate(from.Date));
        SqliteSession.Add(cmd, "@to", SqliteHelpers.WriteDate(to.Date.AddDays(1)));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<DailySalesPoint>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            list.Add(new DailySalesPoint
            {
                Date = DateTime.Parse(reader.GetString(0), System.Globalization.CultureInfo.InvariantCulture),
                Total = AbayPOS.Domain.Money.FromCents(reader.GetInt64(1)),
                Transactions = reader.GetInt32(2)
            });
        }

        return list;
    }

    public async Task<IReadOnlyList<TopProductRow>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT i.ProductId, i.ProductName, SUM(i.Qty) AS QtySold, SUM(i.LineTotalCents) AS Revenue
            FROM SaleItems i
            JOIN Sales s ON s.Id = i.SaleId
            WHERE s.Status = 'Completed' AND s.SaleDate >= @from AND s.SaleDate < @to
            GROUP BY i.ProductId, i.ProductName
            ORDER BY QtySold DESC
            LIMIT @take;
            """);
        SqliteSession.Add(cmd, "@from", SqliteHelpers.WriteDate(from.Date));
        SqliteSession.Add(cmd, "@to", SqliteHelpers.WriteDate(to.Date.AddDays(1)));
        SqliteSession.Add(cmd, "@take", take);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<TopProductRow>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            list.Add(new TopProductRow
            {
                ProductId = reader.GetInt32(0),
                ProductName = reader.GetString(1),
                QtySold = reader.GetInt32(2),
                Revenue = AbayPOS.Domain.Money.FromCents(reader.GetInt64(3))
            });
        }

        return list;
    }

    public async Task<SalesSummary> GetSalesSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT COALESCE(SUM(SubtotalCents), 0), COALESCE(SUM(DiscountCents), 0),
                   COALESCE(SUM(TotalCents), 0), COUNT(*)
            FROM Sales
            WHERE Status = 'Completed' AND SaleDate >= @from AND SaleDate < @to;
            """);
        SqliteSession.Add(cmd, "@from", SqliteHelpers.WriteDate(from.Date));
        SqliteSession.Add(cmd, "@to", SqliteHelpers.WriteDate(to.Date.AddDays(1)));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        var subtotal = AbayPOS.Domain.Money.FromCents(reader.GetInt64(0));
        var discount = AbayPOS.Domain.Money.FromCents(reader.GetInt64(1));
        var total = AbayPOS.Domain.Money.FromCents(reader.GetInt64(2));
        var count = reader.GetInt32(3);
        return new SalesSummary
        {
            From = from.Date,
            To = to.Date,
            Subtotal = subtotal,
            Discount = discount,
            Total = total,
            Transactions = count,
            AverageBasket = count == 0 ? 0m : decimal.Round(total / count, 2, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<IReadOnlyList<CashierSalesRow>> GetSalesByCashierAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT u.Id, u.FullName, COUNT(*), COALESCE(SUM(s.TotalCents), 0)
            FROM Sales s
            JOIN Users u ON u.Id = s.CashierId
            WHERE s.Status = 'Completed' AND s.SaleDate >= @from AND s.SaleDate < @to
            GROUP BY u.Id, u.FullName
            ORDER BY SUM(s.TotalCents) DESC;
            """);
        SqliteSession.Add(cmd, "@from", SqliteHelpers.WriteDate(from.Date));
        SqliteSession.Add(cmd, "@to", SqliteHelpers.WriteDate(to.Date.AddDays(1)));
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<CashierSalesRow>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            list.Add(new CashierSalesRow
            {
                CashierId = reader.GetInt32(0),
                CashierName = reader.GetString(1),
                Transactions = reader.GetInt32(2),
                Total = AbayPOS.Domain.Money.FromCents(reader.GetInt64(3))
            });
        }

        return list;
    }

    public async Task<StockValuation> GetStockValuationAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _session.Command("""
            SELECT COALESCE(SUM(StockQty * CostPriceCents), 0),
                   COALESCE(SUM(StockQty * PriceCents), 0),
                   COUNT(*),
                   COALESCE(SUM(StockQty), 0)
            FROM Products
            WHERE IsActive = 1;
            """);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        return new StockValuation
        {
            CostValue = AbayPOS.Domain.Money.FromCents(reader.GetInt64(0)),
            RetailValue = AbayPOS.Domain.Money.FromCents(reader.GetInt64(1)),
            SkuCount = reader.GetInt32(2),
            UnitCount = reader.GetInt32(3)
        };
    }
}
