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
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _context;

        private int GetCompanyId()
        {
            return int.Parse(
                User.FindFirst("CompanyId")!.Value
            );
        }

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        public class SaleRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        // ======================================
        // GET SALES
        // ======================================
        [HttpGet]
        public async Task<IActionResult> GetSales()
        {
            var companyId = GetCompanyId();

            var sales = await _context.StockTransactions
                .Where(s => s.CompanyId == companyId)
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

        // ======================================
        // CREATE SALE
        // ======================================
        [HttpPost]
        public async Task<IActionResult> CreateSale(SaleRequest dto)
        {
            try
            {
                var companyId = GetCompanyId();

                // only find products belonging to this company
                var product = await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == dto.ProductId &&
                        p.CompanyId == companyId);

                if (product == null)
                    return BadRequest("Product not found");

                if (dto.Quantity <= 0)
                    return BadRequest(
                        "Quantity must be greater than 0");

                if (product.QuantityInStock < dto.Quantity)
                    return BadRequest(
                        "Not enough stock");

                // reduce stock
                product.QuantityInStock -= dto.Quantity;

                // record transaction
                var sale = new StockTransaction
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    Type = "SALE",
                    Date = DateTime.UtcNow,

                    // multi-tenancy
                    CompanyId = companyId
                };

                _context.StockTransactions.Add(sale);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Sale recorded successfully",
                    sale.Id,
                    sale.ProductId,
                    sale.Quantity,
                    sale.Type,
                    sale.Date
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}