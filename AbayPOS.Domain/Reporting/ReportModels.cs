namespace AbayPOS.Domain.Reporting;

/// <summary>Headline numbers for the manager dashboard.</summary>
public sealed class DashboardKpis
{
    public decimal TodaySales { get; set; }
    public int TodayTransactions { get; set; }
    public decimal AverageBasket { get; set; }
    public int LowStockCount { get; set; }
}

/// <summary>One day of completed sales.</summary>
public sealed class DailySalesPoint
{
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public int Transactions { get; set; }
}

/// <summary>Ranked product by quantity sold.</summary>
public sealed class TopProductRow
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QtySold { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>Aggregated sales for a date range.</summary>
public sealed class SalesSummary
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public int Transactions { get; set; }
    public decimal AverageBasket { get; set; }
}

/// <summary>Sales grouped by cashier.</summary>
public sealed class CashierSalesRow
{
    public int CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public int Transactions { get; set; }
    public decimal Total { get; set; }
}

/// <summary>Inventory valued at cost.</summary>
public sealed class StockValuation
{
    public decimal CostValue { get; set; }
    public decimal RetailValue { get; set; }
    public int SkuCount { get; set; }
    public int UnitCount { get; set; }
}
