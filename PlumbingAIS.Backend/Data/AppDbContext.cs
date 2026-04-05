using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        // Інші таблиці (Users, Transactions тощо) 
    }
}