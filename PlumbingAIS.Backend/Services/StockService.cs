using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Services
{
    public class StockService : IStockService
    {
        private readonly IGenericRepository<Stock> _stockRepo;
        private readonly IGenericRepository<Transaction> _transRepo;
        private readonly IProductRepository _productRepo;

        public StockService(
            IGenericRepository<Stock> stockRepo,
            IGenericRepository<Transaction> transRepo,
            IProductRepository productRepo)
        {
            _stockRepo = stockRepo;
            _transRepo = transRepo;
            _productRepo = productRepo;
        }

        public async Task<bool> ProcessTransactionAsync(int productId, int locationId, decimal quantity, string type, int userId)
        {
            
            var stocks = await _stockRepo.GetAllAsync();
            var stock = stocks.FirstOrDefault(s => s.ProductId == productId && s.LocationId == locationId);

            bool isNew = false;

            if (stock == null)
            {
                
                stock = new Stock { ProductId = productId, LocationId = locationId, Quantity = 0 };
                await _stockRepo.AddAsync(stock);
                isNew = true;
            }

            // Логіка зміни кількості
            if (type.ToLower() == "in")
            {
                stock.Quantity += quantity;
            }
            else if (type.ToLower() == "out")
            {
                if (stock.Quantity < quantity) return false; 
                stock.Quantity -= quantity;
            }

            
            if (!isNew)
            {
                _stockRepo.Update(stock);
            }

            
            var transaction = new Transaction
            {
                Type = type,
                UserId = userId,
                Date = DateTime.Now,
                DocumentNumber = $"TRX-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            await _transRepo.AddAsync(transaction);

            
            await _stockRepo.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<object>> GetCriticalStocksAsync()
        {
            var products = await _productRepo.GetAllAsync();
            var stocks = await _stockRepo.GetAllAsync();

            return products
                .Select(p => new {
                    p.Name,
                    p.SKU,
                    CurrentQuantity = stocks.Where(s => s.ProductId == p.Id).Sum(s => s.Quantity),
                    p.MinThreshold
                })
                .Where(x => x.CurrentQuantity <= x.MinThreshold);
        }
    }
}