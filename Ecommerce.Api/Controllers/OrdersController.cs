// Ecommerce.Api/Controllers/OrdersController.cs
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idStr, out userId);
    }

    // GET /api/orders/my
    [HttpGet("my")]
    public async Task<IActionResult> MyOrders()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.Status,
                o.TotalUsd,
                o.CreatedAt,
                Items = o.Items.Select(i => new
                {
                    i.ItemType,
                    i.Quantity,
                    i.UnitPriceUsd,
                    i.LineTotalUsd,
                    Product = i.ProductId != null ? new { i.ProductId, i.Product!.Title, i.Product!.Slug } : null,
                    Service = i.ServiceId != null ? new { i.ServiceId, i.Service!.Title, i.Service!.Slug } : null,
                    Package = i.PackageId != null ? new { i.PackageId, i.Package!.Name } : null,
                    i.ServiceRequestId
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    // GET /api/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == id && o.UserId == userId)
            .Select(o => new
            {
                o.Id,
                o.Status,
                o.TotalUsd,
                o.CreatedAt,
                Items = o.Items.Select(i => new
                {
                    i.ItemType,
                    i.Quantity,
                    i.UnitPriceUsd,
                    i.LineTotalUsd,
                    Product = i.ProductId != null ? new { i.ProductId, i.Product!.Title, i.Product!.Slug } : null,
                    Service = i.ServiceId != null ? new { i.ServiceId, i.Service!.Title, i.Service!.Slug } : null,
                    Package = i.PackageId != null ? new { i.PackageId, i.Package!.Name, i.Package!.PriceUsd } : null,
                    i.ServiceRequestId
                }).ToList(),
                Payments = o.Payments.Select(p => new { p.Id, p.Provider, p.Status, p.ProviderRef, p.AmountUsd, p.CreatedAt }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null) return NotFound();
        return Ok(order);
    }

    // DELETE /api/orders/my
    // يحذف كل طلبات المستخدم الحالي (للتنظيف أثناء التطوير)
    [HttpDelete("my")]
    public async Task<IActionResult> DeleteMyOrders()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();

        if (orders.Count == 0) return Ok(new { deleted = 0 });

        _db.Orders.RemoveRange(orders);
        var deleted = await _db.SaveChangesAsync();

        // deleted هنا يعكس عدد العمليات على EF، مو عدد الطلبات بالضرورة
        return Ok(new { deletedOrders = orders.Count, affected = deleted });
    }
}
