using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/purchase-invoices")]
[Authorize(Roles = "Admin")]
public class AdminPurchaseInvoicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminPurchaseInvoicesController(AppDbContext db)
    {
        _db = db;
    }

    private static string ShortCode(Guid id) => id.ToString("N")[..8].ToUpperInvariant();

    private static string NormalizeStatus(string? status)
    {
        var s = (status ?? "").Trim();
        if (s.Equals("Paid", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("Succeeded", StringComparison.OrdinalIgnoreCase)) return "Sold";
        if (s.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("Canceled", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("NotSold", StringComparison.OrdinalIgnoreCase)) return "NotSold";
        if (s.Equals("Sold", StringComparison.OrdinalIgnoreCase)) return "Sold";
        return "PendingSale";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? search = null)
    {
        try
        {
            var q = _db.Orders.AsNoTracking()
                .Where(o => o.Items.Any(i => i.ProductId != null));

            var normalizedStatus = (status ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(normalizedStatus) && !normalizedStatus.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (normalizedStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(o => o.Status == "Sold" || o.Status == "Paid" || o.Status == "Completed");
                else if (normalizedStatus.Equals("NotSold", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(o => o.Status == "NotSold" || o.Status == "Cancelled" || o.Status == "Canceled");
                else
                    q = q.Where(o => o.Status == "PendingSale" || o.Status == "PendingPayment" || o.Status == "Pending");
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                q = q.Where(o =>
                    o.User.Email.ToLower().Contains(term) ||
                    o.User.FullName.ToLower().Contains(term) ||
                    o.Items.Any(i => i.Product != null && i.Product.Title.ToLower().Contains(term))
                );
            }

            var invoices = await q.OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    code = "",
                    status = o.Status,
                    rawStatus = o.Status,
                    o.SubtotalIqd,
                    o.DiscountAmountIqd,
                    o.TotalIqd,
                    o.TotalUsd,
                    o.ProfitIqd,
                    o.ProfitUsd,
                    o.CreatedAt,
                    o.SoldAt,
                    customer = new
                    {
                        o.UserId,
                        name = o.User.FullName,
                        email = o.User.Email
                    },
                    items = o.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        title = i.Product != null ? i.Product.Title : "منتج محذوف",
                        slug = i.Product != null ? i.Product.Slug : "",
                        brand = i.Product != null ? i.Product.Brand : "",
                        i.Quantity,
                        i.UnitPriceIqd,
                        i.LineTotalIqd,
                        stockQuantity = i.Product != null ? i.Product.StockQuantity : 0
                    }).ToList(),
                    primaryItemTitle = o.Items.Select(i => i.Product != null ? i.Product.Title : null).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(invoices);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load purchase invoices.", detail = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOne(Guid id)
    {
        var invoice = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == id && o.Items.Any(i => i.ProductId != null))
            .Select(o => new
            {
                o.Id,
                code = "",
                status = o.Status,
                rawStatus = o.Status,
                o.SubtotalIqd,
                o.DiscountAmountIqd,
                o.TotalIqd,
                o.TotalUsd,
                o.ProfitIqd,
                o.ProfitUsd,
                o.CreatedAt,
                o.SoldAt,
                customer = new { o.UserId, name = o.User.FullName, email = o.User.Email },
                items = o.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    title = i.Product != null ? i.Product.Title : "منتج محذوف",
                    slug = i.Product != null ? i.Product.Slug : "",
                    brand = i.Product != null ? i.Product.Brand : "",
                    i.Quantity,
                    i.UnitPriceIqd,
                    i.LineTotalIqd,
                    stockQuantity = i.Product != null ? i.Product.StockQuantity : 0
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return invoice == null ? NotFound(new { message = "Invoice not found." }) : Ok(invoice);
    }

    [HttpPost("{id:guid}/sold")]
    public async Task<IActionResult> MarkSold(Guid id)
    {
        try
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Invoice not found." });

            var normalized = NormalizeStatus(order.Status);
            if (normalized == "NotSold") return BadRequest(new { message = "هذه الفاتورة ملغاة ولا يمكن تحويلها إلى تم البيع." });

            var alreadySold = normalized == "Sold";
            if (!alreadySold)
            {
                foreach (var item in order.Items.Where(x => x.ProductId != null))
                {
                    if (item.Product == null) continue;
                    if (item.Product.StockQuantity < item.Quantity)
                    {
                        return BadRequest(new
                        {
                            message = "الكمية غير كافية لإتمام البيع.",
                            productId = item.ProductId,
                            productTitle = item.Product.Title,
                            available = item.Product.StockQuantity,
                            required = item.Quantity
                        });
                    }
                }

                foreach (var item in order.Items.Where(x => x.ProductId != null))
                {
                    if (item.Product == null) continue;
                    item.Product.StockQuantity = Math.Max(0, item.Product.StockQuantity - item.Quantity);
                }
            }

            order.Status = "Sold";
            order.SoldAt ??= DateTime.UtcNow;
            order.ProfitIqd = order.TotalIqd;
            order.ProfitUsd = order.TotalUsd;

            foreach (var p in order.Payments)
            {
                p.Status = "Succeeded";
            }

            await _db.SaveChangesAsync();
            return Ok(new { id = order.Id, code = ShortCode(order.Id), status = order.Status, profitIqd = order.ProfitIqd, soldAt = order.SoldAt });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to confirm invoice sale.", detail = ex.Message });
        }
    }

    [HttpPost("{id:guid}/not-sold")]
    public async Task<IActionResult> MarkNotSold(Guid id)
    {
        try
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Invoice not found." });

            var normalized = NormalizeStatus(order.Status);
            if (normalized == "Sold") return BadRequest(new { message = "الفاتورة مكتملة البيع ولا يمكن حذفها من هنا." });

            var couponUsages = await _db.CouponUsages.Where(x => x.OrderId == id).ToListAsync();
            var downloadTokens = await _db.DownloadTokens.Where(x => x.OrderId == id).ToListAsync();

            if (couponUsages.Count > 0) _db.CouponUsages.RemoveRange(couponUsages);
            if (downloadTokens.Count > 0) _db.DownloadTokens.RemoveRange(downloadTokens);
            if (order.Items.Count > 0) _db.OrderItems.RemoveRange(order.Items);
            if (order.Payments.Count > 0) _db.Payments.RemoveRange(order.Payments);
            _db.Orders.Remove(order);

            await _db.SaveChangesAsync();
            return Ok(new { id, status = "NotSold", deleted = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to cancel invoice.", detail = ex.Message });
        }
    }
}
