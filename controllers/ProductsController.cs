using Microsoft.AspNetCore.Mvc;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InventoryApi.Controllers
{    [Authorize]
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
public async Task<IActionResult> AddProduct(Product product)
{
    if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.SKU))
    {
        return BadRequest("Product Name and SKU are required");
    }

    // normalize
    product.Name = product.Name.ToUpper();
    product.SKU = product.SKU.ToUpper();

    // 🔍 check duplicate SKU (primary rule)
    var exists = await _context.Products.AnyAsync(p =>
        p.SKU == product.SKU
    );

    if (exists)
    {
        return BadRequest("Product with this SKU already exists");
    }

    // OPTIONAL: also prevent duplicate name + SKU combo
    var nameExists = await _context.Products.AnyAsync(p =>
        p.Name == product.Name && p.SKU == product.SKU
    );

    if (nameExists)
    {
        return BadRequest("Product already exists");
    }

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
[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

    }
}   




    
