using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Services
{
    public class StockService : IStockService
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepo;

        public StockService(AppDbContext context, IProductRepository productRepo)
        {
            _context = context;
            _productRepo = productRepo;
        }

        public async Task<int> ProcessGroupTransactionAsync(TransactionRequestDto request, int userId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transaction = new Transaction
                {
                    Type = request.Type,
                    UserId = userId,
                    ContractorId = request.ContractorId,
                    Date = DateTime.Now,
                    Description = request.Description,
                    DocumentNumber = $"TRX-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                foreach (var item in request.Items)
                {
                    var stock = await _context.Stocks
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.LocationId == item.LocationId);

                    bool isIncoming = request.Type.ToLower().Contains("in");
                    bool isMoveTo = request.Type.Equals("Move", StringComparison.OrdinalIgnoreCase) && item == request.Items.Last();

                    if (isIncoming || isMoveTo)
                    {
                        if (stock == null)
                        {
                            stock = new Stock { ProductId = item.ProductId, LocationId = item.LocationId, Quantity = item.Quantity };
                            _context.Stocks.Add(stock);
                        }
                        else
                        {
                            stock.Quantity += item.Quantity;
                        }
                    }
                    else
                    {
                        if (stock == null || stock.Quantity < item.Quantity)
                            throw new Exception("Insufficient stock");

                        stock.Quantity -= item.Quantity;
                    }

                    _context.TransactionItems.Add(new TransactionItem
                    {
                        TransactionId = transaction.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PriceAtTime = item.Price
                    });
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return transaction.Id;
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                return 0;
            }
        }

        public async Task<int> MoveStockAsync(int productId, int fromLocationId, int toLocationId, decimal quantity, int userId, string? description = null)
        {
            if (string.IsNullOrEmpty(description))
            {
                var fromLoc = await _context.Locations.Include(l => l.Warehouse).FirstOrDefaultAsync(l => l.Id == fromLocationId);
                var toLoc = await _context.Locations.Include(l => l.Warehouse).FirstOrDefaultAsync(l => l.Id == toLocationId);

                description = $"Маршрут: {fromLoc?.Warehouse?.Name} ({fromLoc?.RowCode}-{fromLoc?.RackCode}-{fromLoc?.ShelfCode}) → {toLoc?.Warehouse?.Name} ({toLoc?.RowCode}-{toLoc?.RackCode}-{toLoc?.ShelfCode})";
            }

            var moveRequest = new TransactionRequestDto
            {
                Type = "Move",
                Description = description,
                Items = new List<TransactionItemRequestDto>
                {
                    new TransactionItemRequestDto { ProductId = productId, LocationId = fromLocationId, Quantity = quantity, Price = 0 },
                    new TransactionItemRequestDto { ProductId = productId, LocationId = toLocationId, Quantity = quantity, Price = 0 }
                }
            };

            return await ProcessGroupTransactionAsync(moveRequest, userId);
        }

        public async Task<IEnumerable<object>> GetCriticalStocksAsync()
        {
            var products = await _productRepo.GetAllAsync();
            var stocks = await _context.Stocks.ToListAsync();
            return products
                .Select(p => new {
                    p.Name,
                    p.SKU,
                    CurrentQuantity = stocks.Where(s => s.ProductId == p.Id).Sum(s => s.Quantity),
                    p.MinThreshold
                })
                .Where(x => x.CurrentQuantity <= x.MinThreshold);
        }

        public async Task<decimal> GetTotalStockValueAsync()
        {
            var stocks = await _context.Stocks.Include(s => s.Product).ToListAsync();
            return stocks.Sum(s => s.Quantity * (s.Product?.Price ?? 0));
        }
    }
}