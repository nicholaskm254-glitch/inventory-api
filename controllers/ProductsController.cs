using Microsoft.AspNetCore.Mvc;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET ALL PRODUCTS
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            return Ok(await _context.Products.ToListAsync());
        }

        // ADD PRODUCT
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // ADD STOCK TO EXISTING PRODUCT
     [HttpPut("{id}/add-stock")]
public async Task<IActionResult> AddStock(int id, [FromBody] int quantity)
{
    var product = await _context.Products.FindAsync(id);

    if (product == null)
        return NotFound("Product not found");

    // FIX NULL SAFETY
    product.QuantityInStock += quantity;

    var transaction = new StockTransaction
    {
        ProductId = product.Id,
        Quantity = quantity,
        Type = "RESTOCK",
        Date = DateTime.UtcNow
    };

    _context.StockTransactions.Add(transaction);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Stock updated",
        newStock = product.QuantityInStock
    });
}
    }
}   




    
