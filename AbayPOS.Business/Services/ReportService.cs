using AbayPOS.Data;
using AbayPOS.Domain.Reporting;

namespace AbayPOS.Business.Services;

/// <summary>Read-only reporting for the manager dashboard and CSV export.</summary>
public interface IReportService
{
    Task<DashboardKpis> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailySalesPoint>> GetLast7DaysAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailySalesPoint>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopProductRow>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken cancellationToken = default);
    Task<SalesSummary> GetSalesSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashierSalesRow>> GetSalesByCashierAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<StockValuation> GetStockValuationAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class ReportService : IReportService
{
    private readonly IUnitOfWorkFactory _uow;

    public ReportService(IUnitOfWorkFactory uow) => _uow = uow;

    public async Task<DashboardKpis> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Reports.GetDashboardAsync(DateTime.Now, cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<DailySalesPoint>> GetLast7DaysAsync(CancellationToken cancellationToken = default)
    {
        var to = DateTime.Now.Date;
        return GetDailySalesAsync(to.AddDays(-6), to, cancellationToken);
    }

    public async Task<IReadOnlyList<DailySalesPoint>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        from = from.Date;
        to = to.Date;
        if (to < from)
            (from, to) = (to, from);

        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        var points = await uow.Reports.GetDailySalesAsync(from, to, cancellationToken).ConfigureAwait(false);
        var filled = new List<DailySalesPoint>();
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            var match = points.FirstOrDefault(p => p.Date.Date == day);
            filled.Add(match ?? new DailySalesPoint { Date = day });
        }

        return filled;
    }

    public async Task<IReadOnlyList<TopProductRow>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Reports.GetTopProductsAsync(from, to, take, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SalesSummary> GetSalesSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Reports.GetSalesSummaryAsync(from, to, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CashierSalesRow>> GetSalesByCashierAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Reports.GetSalesByCashierAsync(from, to, cancellationToken).ConfigureAwait(false);
    }

    public async Task<StockValuation> GetStockValuationAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uow.Create();
        await uow.EnsureOpenAsync(cancellationToken).ConfigureAwait(false);
        return await uow.Reports.GetStockValuationAsync(cancellationToken).ConfigureAwait(false);
    }
}
