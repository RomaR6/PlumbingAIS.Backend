using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IGenericRepository<Product> Products { get; private set; }
        public IGenericRepository<Stock> Stocks { get; private set; }
        public IGenericRepository<Transaction> Transactions { get; private set; }
        public IGenericRepository<TransactionItem> TransactionItems { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Products = new GenericRepository<Product>(_context);
            Stocks = new GenericRepository<Stock>(_context);
            Transactions = new GenericRepository<Transaction>(_context);
            TransactionItems = new GenericRepository<TransactionItem>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}