using Microsoft.AspNetCore.Mvc;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InventoryApi.Controllers
{   [Authorize]
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
      [HttpPost]
public async Task<IActionResult> CreateSale(SaleRequest dto)
{
    try
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
            return BadRequest("Product not found");

        if (dto.Quantity <= 0)
            return BadRequest("Quantity must be greater than 0");

        if (product.QuantityInStock < dto.Quantity)
            return BadRequest("Not enough stock");

        // ✅ SIMPLE DIRECT UPDATE (NO REFLECTION)
        product.QuantityInStock -= dto.Quantity;

        var sale = new StockTransaction
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Type = "Sale",
            Date = DateTime.UtcNow
        };

        _context.StockTransactions.Add(sale);

        await _context.SaveChangesAsync();

        return Ok(sale);
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.ToString());
    }
}

    }
}