using Microsoft.AspNetCore.Mvc;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InventoryApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        private int GetCompanyId()
        {
            return int.Parse(
                User.FindFirst("CompanyId")!.Value
            );
        }

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // GET ALL PRODUCTS
        // ============================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var companyId = GetCompanyId();

            var products = await _context.Products
                .Where(p => p.CompanyId == companyId)
                .ToListAsync();

            return Ok(products);
        }

        // ============================
        // ADD PRODUCT
        // ============================
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (product == null)
                return BadRequest("Invalid product");

            if (string.IsNullOrWhiteSpace(product.Name) ||
                string.IsNullOrWhiteSpace(product.SKU))
            {
                return BadRequest(
                    "Product Name and SKU are required"
                );
            }

            var companyId = GetCompanyId();

            // normalize
            product.Name = product.Name
                .Trim()
                .ToUpper();

            product.SKU = product.SKU
                .Trim()
                .ToUpper();

            // check duplicate SKU within company
            var exists = await _context.Products
                .AnyAsync(p =>
                    p.CompanyId == companyId &&
                    p.SKU == product.SKU);

            if (exists)
            {
                return BadRequest(
                    "Product with this SKU already exists"
                );
            }

            // assign tenant
            product.CompanyId = companyId;

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // ============================
        // ADD STOCK
        // ============================
        [HttpPut("{id}/add-stock")]
        public async Task<IActionResult> AddStock(
            int id,
            [FromBody] int quantity)
        {
            var companyId = GetCompanyId();

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.CompanyId == companyId);

            if (product == null)
                return NotFound(
                    "Product not found"
                );

            product.QuantityInStock += quantity;

            var transaction =
                new StockTransaction
                {
                    ProductId = product.Id,
                    CompanyId = companyId,
                    Quantity = quantity,
                    Type = "RESTOCK",
                    Date = DateTime.UtcNow
                };

            _context.StockTransactions
                .Add(transaction);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Stock updated",
                newStock = product.QuantityInStock
            });
        }

        // ============================
        // DELETE PRODUCT
        // ============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(
            int id)
        {
            var companyId = GetCompanyId();

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.CompanyId == companyId);

            if (product == null)
                return NotFound(
                    "Product not found"
                );

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Product deleted successfully"
            });
        }
    }
}