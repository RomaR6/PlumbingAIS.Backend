using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.DTOs;

namespace PlumbingAIS.Backend.Interfaces
{
    public delegate void LowStockHandler(object sender, LowStockEventArgs e);

    public interface IStockService
    {
        event LowStockHandler? OnLowStockReached;
        Task<int> ProcessGroupTransactionAsync(TransactionRequestDto request, int userId);
        Task<IEnumerable<InventoryReportItem>> GetCriticalStocksAsync();
        Task<decimal> GetTotalStockValueAsync();
        Task<int> MoveStockAsync(int productId, int fromLocationId, int toLocationId, decimal quantity, int userId, string? description = null);
    }
}