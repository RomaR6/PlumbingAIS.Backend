using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.DTOs;

namespace PlumbingAIS.Backend.Interfaces
{
    public interface IStockService
    {
        Task<int> ProcessGroupTransactionAsync(TransactionRequestDto request, int userId);
        Task<IEnumerable<object>> GetCriticalStocksAsync();
        Task<decimal> GetTotalStockValueAsync();
        Task<int> MoveStockAsync(int productId, int fromLocationId, int toLocationId, decimal quantity, int userId, string? description = null);
    }
}