using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly AppDbContext _db;

    public CheckoutController(AppDbContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idStr, out userId);
    }

    // POST /api/checkout/products
    [HttpPost("products")]
    public async Task<IActionResult> CheckoutProduct([FromBody] CheckoutProductRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId && p.IsPublished);
        if (product == null) return NotFound("Product not found");

        var qty = req.Quantity <= 0 ? 1 : req.Quantity;
        if (qty > 50) return BadRequest("Quantity too large");

        // إنشاء Order
        var order = new Order
        {
            UserId = userId,
            Status = "PendingPayment",
            TotalUsd = product.PriceUsd * qty
        };

        order.Items.Add(new OrderItem
        {
            ItemType = "DigitalProduct",
            ProductId = product.Id,
            UnitPriceUsd = product.PriceUsd,
            Quantity = qty,
            LineTotalUsd = product.PriceUsd * qty
        });

        // إنشاء Payment (Mock)
        var payment = new Payment
        {
            Order = order,
            Provider = "Mock",
            Status = "Pending",
            ProviderRef = $"MOCK-{Guid.NewGuid():N}",
            AmountUsd = order.TotalUsd
        };

        order.Payments.Add(payment);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            payment = new { payment.Id, payment.Provider, payment.Status, payment.ProviderRef }
        });
    }

    // POST /api/checkout/services
    // هذا ينشئ Order مرتبط بـ ServiceRequest موجود (اللي سويته سابقًا)
    [HttpPost("services")]
    public async Task<IActionResult> CheckoutService([FromBody] CheckoutServiceRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var sr = await _db.ServiceRequests
            .Include(x => x.Package)
            .Include(x => x.Service)
            .FirstOrDefaultAsync(x => x.Id == req.ServiceRequestId && x.UserId == userId);

        if (sr == null) return NotFound("ServiceRequest not found");

        if (sr.Status != "PendingPayment")
            return BadRequest("ServiceRequest is not pending payment");

        var price = sr.Package.PriceUsd;

        var order = new Order
        {
            UserId = userId,
            Status = "PendingPayment",
            TotalUsd = price
        };

        order.Items.Add(new OrderItem
        {
            ItemType = "Service",
            ServiceId = sr.ServiceId,
            PackageId = sr.PackageId,
            ServiceRequestId = sr.Id,
            UnitPriceUsd = price,
            Quantity = 1,
            LineTotalUsd = price
        });

        var payment = new Payment
        {
            Order = order,
            Provider = "Mock",
            Status = "Pending",
            ProviderRef = $"MOCK-{Guid.NewGuid():N}",
            AmountUsd = order.TotalUsd
        };

        order.Payments.Add(payment);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            service = new { sr.Id, sr.Service.Title, package = sr.Package.Name },
            payment = new { payment.Id, payment.Provider, payment.Status, payment.ProviderRef }
        });
    }
}

public class CheckoutProductRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CheckoutServiceRequest
{
    public Guid ServiceRequestId { get; set; }
}
