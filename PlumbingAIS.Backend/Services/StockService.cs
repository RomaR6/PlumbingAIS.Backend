using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Services
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        public event LowStockHandler? OnLowStockReached;

        public StockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> ProcessGroupTransactionAsync(TransactionRequestDto request, int userId)
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

            await _unitOfWork.Transactions.AddAsync(transaction);

            foreach (var item in request.Items)
            {
                var stocks = await _unitOfWork.Stocks.GetAllAsync();
                var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId && s.LocationId == item.LocationId);

                bool isIncoming = request.Type.Equals("In", StringComparison.OrdinalIgnoreCase);
                bool isOutgoing = request.Type.Equals("Out", StringComparison.OrdinalIgnoreCase);
                bool isMove = request.Type.Equals("Move", StringComparison.OrdinalIgnoreCase);

                if (isIncoming || (isMove && item == request.Items.Last()))
                {
                    if (stock == null)
                    {
                        
                        stock = new Stock { ProductId = item.ProductId, LocationId = item.LocationId };
                        stock.AddQuantity(item.Quantity);
                        await _unitOfWork.Stocks.AddAsync(stock);
                    }
                    else
                    {
                        stock.AddQuantity(item.Quantity);
                        _unitOfWork.Stocks.Update(stock);
                    }
                }
                else if (isOutgoing || (isMove && item == request.Items.First()))
                {
                    if (stock == null || stock.Quantity < item.Quantity)
                        throw new Exception($"Недостатньо товару на складі (Продукт ID:{item.ProductId}, Локація ID:{item.LocationId})");

                    stock.RemoveQuantity(item.Quantity);

                    if (stock.Quantity <= 0)
                        _unitOfWork.Stocks.Delete(stock);
                    else
                        _unitOfWork.Stocks.Update(stock);

                    await CheckAndNotifyLowStock(item.ProductId);
                }

                
                await _unitOfWork.TransactionItems.AddAsync(new TransactionItem
                {
                    Transaction = transaction,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtTime = item.Price
                });
            }

            await _unitOfWork.CompleteAsync();
            return transaction.Id;
        }

        private async Task CheckAndNotifyLowStock(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return;

            var allStocks = await _unitOfWork.Stocks.GetAllAsync();
            var totalQuantity = allStocks.Where(s => s.ProductId == productId).Sum(s => s.Quantity);

            if (totalQuantity < product.MinThreshold)
            {
                OnLowStockReached?.Invoke(this, new LowStockEventArgs
                {
                    ProductName = product.Name,
                    SKU = product.SKU,
                    CurrentQuantity = totalQuantity,
                    Threshold = product.MinThreshold
                });
            }
        }

        public async Task<int> MoveStockAsync(int productId, int fromLocationId, int toLocationId, decimal quantity, int userId, string? description = null)
        {
            var moveRequest = new TransactionRequestDto
            {
                Type = "Move",
                Description = description ?? "Внутрішнє переміщення",
                Items = new List<TransactionItemRequestDto>
                {
                    new TransactionItemRequestDto { ProductId = productId, LocationId = fromLocationId, Quantity = quantity, Price = 0 },
                    new TransactionItemRequestDto { ProductId = productId, LocationId = toLocationId, Quantity = quantity, Price = 0 }
                }
            };
            return await ProcessGroupTransactionAsync(moveRequest, userId);
        }

        public async Task<IEnumerable<InventoryReportItem>> GetCriticalStocksAsync()
        {
            var stocks = await _unitOfWork.Stocks.GetAllAsync(s => s.Product);
            return stocks
                .GroupBy(s => s.ProductId)
                .Select(group => new InventoryReportItem
                {
                    Name = group.First().Product?.Name ?? "Unknown",
                    SKU = group.First().Product?.SKU ?? "—",
                    CurrentQuantity = group.Sum(s => s.Quantity),
                    MinThreshold = group.First().Product?.MinThreshold ?? 0
                })
                .Where(x => x.CurrentQuantity < x.MinThreshold)
                .ToList();
        }

        public async Task<decimal> GetTotalStockValueAsync()
        {
            var stocks = await _unitOfWork.Stocks.GetAllAsync(s => s.Product);
            return stocks.Sum(s => (s.Quantity * (s.Product?.Price ?? 0)));
        }
    }
}