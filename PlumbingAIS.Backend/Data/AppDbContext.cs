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
        public DbSet<TransactionItem> TransactionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" },
                new Role { Id = 3, Name = "Manager" }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Труби" },
                new Category { Id = 2, Name = "Фітинги" },
                new Category { Id = 3, Name = "Змішувачі" },
                new Category { Id = 4, Name = "Кераміка" },
                new Category { Id = 5, Name = "Змішувачі для кухні" },
                new Category { Id = 6, Name = "Душові системи" },
                new Category { Id = 7, Name = "Інсталяції" },
                new Category { Id = 8, Name = "Рушникосушки" },
                new Category { Id = 9, Name = "Запірна арматура" }
            );

            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "EcoPlast" },
                new Brand { Id = 2, Name = "Grohe" },
                new Brand { Id = 3, Name = "Wavin" },
                new Brand { Id = 4, Name = "Kisan" },
                new Brand { Id = 5, Name = "Hansa" },
                new Brand { Id = 6, Name = "Valtec" },
                new Brand { Id = 7, Name = "Geberit" },
                new Brand { Id = 8, Name = "Kolo" },
                new Brand { Id = 9, Name = "Cersanit" },
                new Brand { Id = 10, Name = "Imprese" },
                new Brand { Id = 11, Name = "Villeroy & Boch" },
                new Brand { Id = 12, Name = "Franke" }
            );

            modelBuilder.Entity<Unit>().HasData(
                new Unit { Id = 1, Name = "шт" },
                new Unit { Id = 2, Name = "м" },
                new Unit { Id = 3, Name = "комплект" }
            );
        }
    }
}