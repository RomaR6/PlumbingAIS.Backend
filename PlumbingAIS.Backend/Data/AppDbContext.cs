using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Contractor> Contractors { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }

    }
}