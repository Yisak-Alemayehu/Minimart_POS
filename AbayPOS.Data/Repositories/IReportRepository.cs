using AbayPOS.Domain.Reporting;

namespace AbayPOS.Data.Repositories;

public interface IReportRepository
{
    Task<DashboardKpis> GetDashboardAsync(DateTime today, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailySalesPoint>> GetDailySalesAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopProductRow>> GetTopProductsAsync(DateTime from, DateTime to, int take, CancellationToken cancellationToken = default);
    Task<SalesSummary> GetSalesSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashierSalesRow>> GetSalesByCashierAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<StockValuation> GetStockValuationAsync(CancellationToken cancellationToken = default);
}
