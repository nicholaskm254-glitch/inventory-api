using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }

    }
}