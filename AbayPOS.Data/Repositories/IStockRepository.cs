using AbayPOS.Domain.Entities;

namespace AbayPOS.Data.Repositories;

public interface IStockRepository
{
    Task<int> InsertReceiptAsync(StockReceipt receipt, CancellationToken cancellationToken = default);
    Task InsertReceiptItemAsync(StockReceiptItem item, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockReceipt>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
}
