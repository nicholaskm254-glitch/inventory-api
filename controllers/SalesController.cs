using Microsoft.AspNetCore.Mvc;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        public class SaleRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
[HttpGet]
public async Task<IActionResult> GetSales()
{
    var sales = await _context.StockTransactions
        .Include(s => s.Product)
        .OrderByDescending(s => s.Date)
        .Select(s => new
        {
            id = s.Id,
            productId = s.ProductId,
            productName = s.Product.Name,
            price = s.Product.Price,
            quantity = s.Quantity,
            type = s.Type,
            date = s.Date
        })
        .ToListAsync();

    return Ok(sales);
}
        [HttpPost]
        public async Task<IActionResult> SellProduct([FromBody] SaleRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);

            if (product == null)
                return NotFound("Product not found");

            if (product.QuantityInStock < request.Quantity)
                return BadRequest("Not enough stock");

            product.QuantityInStock -= request.Quantity;

            var transaction = new StockTransaction
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Type = "SALE",
                Date = DateTime.Now
            };

            _context.StockTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Sale completed" });
        }
    }
}