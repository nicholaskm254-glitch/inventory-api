using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace InventoryApi.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseNpgsql("Host=dpg-d8qffaugvqtc73a6akn0-a.oregon-postgres.render.com;Port=5432;Database=inventory_iw70;Username=nicholas;Password=hlmCentENKLuImDO2aaMpKc6bhDfhaXq;Ssl Mode=Require");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}