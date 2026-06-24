using Microsoft.EntityFrameworkCore;
using InventoryApi.Models;

namespace InventoryApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<StockTransaction> StockTransactions { get; set; }

        // Authentication
        public DbSet<User> Users { get; set; }
    }
}