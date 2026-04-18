using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Interfaces
{
    public interface IStockService
    {
        Task<bool> ProcessTransactionAsync(int productId, int locationId, decimal quantity, string type, int userId);
        Task<IEnumerable<object>> GetCriticalStocksAsync(); 
    }
}