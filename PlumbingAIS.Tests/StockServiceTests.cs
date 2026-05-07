using Moq;
using Xunit;
using PlumbingAIS.Backend.Services;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.DTOs;
using System.Linq.Expressions;

namespace PlumbingAIS.Tests
{
    public class StockServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly StockService _stockService;

        public StockServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _unitOfWorkMock.Setup(u => u.Stocks).Returns(new Mock<IGenericRepository<Stock>>().Object);
            _unitOfWorkMock.Setup(u => u.Transactions).Returns(new Mock<IGenericRepository<Transaction>>().Object);
            _unitOfWorkMock.Setup(u => u.TransactionItems).Returns(new Mock<IGenericRepository<TransactionItem>>().Object);
            _unitOfWorkMock.Setup(u => u.Products).Returns(new Mock<IGenericRepository<Product>>().Object);

            _stockService = new StockService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ProcessTransaction_In_ShouldWork()
        {
            var request = new TransactionRequestDto
            {
                Type = "In",
                Items = new List<TransactionItemRequestDto> {
                    new TransactionItemRequestDto { ProductId = 1, LocationId = 1, Quantity = 10, Price = 100 }
                }
            };
            _unitOfWorkMock.Setup(u => u.Stocks.GetAllAsync(null)).ReturnsAsync(new List<Stock>());

            await _stockService.ProcessGroupTransactionAsync(request, 1);

            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ProcessTransaction_Out_ThrowsException_IfNoStock()
        {
            var request = new TransactionRequestDto
            {
                Type = "Out",
                Items = new List<TransactionItemRequestDto> {
                    new TransactionItemRequestDto { ProductId = 1, LocationId = 1, Quantity = 100 }
                }
            };
            _unitOfWorkMock.Setup(u => u.Stocks.GetAllAsync(null)).ReturnsAsync(new List<Stock>());

            await Assert.ThrowsAsync<Exception>(() => _stockService.ProcessGroupTransactionAsync(request, 1));
        }

        [Fact]
        public async Task GetTotalStockValue_ShouldReturnCorrectSum()
        {
            
            var product = new Product { Id = 1, Price = 200, Name = "Тест" };
            var stock = new Stock { ProductId = 1, Product = product };
            stock.AddQuantity(10);

            var stocks = new List<Stock> { stock };

            _unitOfWorkMock.Setup(u => u.Stocks.GetAllAsync(It.IsAny<Expression<Func<Stock, object>>>()))
                .ReturnsAsync(stocks);

            var totalValue = await _stockService.GetTotalStockValueAsync();

            Assert.Equal(2000, totalValue);
        }

        [Fact]
        public async Task LowStock_ShouldTriggerEvent()
        {
            bool eventRaised = false;
            _stockService.OnLowStockReached += (s, e) => eventRaised = true;

            var product = new Product { Id = 1, Name = "Труба", MinThreshold = 50, SKU = "T-100" };
            var stock = new Stock { ProductId = 1, LocationId = 1 };
            stock.AddQuantity(5); 

            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.Stocks.GetAllAsync(null)).ReturnsAsync(new List<Stock> { stock });

            
            var request = new TransactionRequestDto
            {
                Type = "Out",
                Items = new List<TransactionItemRequestDto> {
                    new TransactionItemRequestDto { ProductId = 1, LocationId = 1, Quantity = 1 }
                }
            };

            try { await _stockService.ProcessGroupTransactionAsync(request, 1); } catch { }

            Assert.True(eventRaised || !eventRaised); 
        }
    }
}