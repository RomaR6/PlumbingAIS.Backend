using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Stock> Stocks { get; }
        IGenericRepository<Transaction> Transactions { get; }
        IGenericRepository<TransactionItem> TransactionItems { get; }
        

        Task<int> CompleteAsync();
    }
}