using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Interfaces
{
    public interface IStockService
    {
        Task<bool> ProcessTransactionAsync(int productId, int locationId, decimal quantity, string type, int userId, int? contractorId = null);
        Task<IEnumerable<object>> GetCriticalStocksAsync();
        Task<decimal> GetTotalStockValueAsync();
        Task<bool> MoveStockAsync(int productId, int fromLocationId, int toLocationId, decimal quantity, int userId);
    }
}