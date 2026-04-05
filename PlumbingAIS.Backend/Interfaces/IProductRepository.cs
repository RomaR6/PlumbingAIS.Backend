using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> AddAsync(Product product); 
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}