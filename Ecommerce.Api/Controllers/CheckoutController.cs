using System.Security.Claims;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public CheckoutController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private bool ValidateCheckoutSecret()
    {
        var expected = _config["CHECKOUT_SECRET"] ?? _config["Checkout:Secret"];
        if (string.IsNullOrWhiteSpace(expected)) return true; // لو ما متوفر، لا نكسر بيئة التطوير

        var provided = Request.Headers["x-checkout-secret"].FirstOrDefault();
        return string.Equals(expected, provided, StringComparison.Ordinal);
    }

    private Guid GetUserIdOrEmpty()
    {
        var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// إنشاء طلب لمنتج واحد.
    /// ملاحظة: حتى يظهر الطلب فوراً في لوحة التحكم/الإحصائيات نعتبره مدفوعاً (Status = "Paid")
    /// وننشئ Payment بحالة "Succeeded".
    /// </summary>
    [HttpPost("product")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckoutProduct([FromBody] CheckoutProductRequest req)
    {
        if (!ValidateCheckoutSecret()) return Unauthorized();

        var qty = Math.Max(1, req.Quantity);
        // ✅ لا نسمح بشراء منتج غير منشور
        // ملاحظة: الـ Product عندك ما بيه IsDeleted / IsActive لذلك نعتمد فقط IsPublished
        var product = await _db.Products.FirstOrDefaultAsync(p =>
            p.Id == req.ProductId && p.IsPublished);
        if (product is null) return NotFound(new { message = "Product not found" });

        var userId = GetUserIdOrEmpty();

        var totalUsd = product.PriceUsd * qty;
        var totalIqd = product.PriceIqd * qty;

        var order = new Order
        {
            UserId = userId,
            Status = "Paid", // ✅ حتى يظهر فوراً بلوحة التحكم
            TotalUsd = totalUsd,
            TotalIqd = totalIqd,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>(),
            Payments = new List<Payment>()
        };

        order.Items.Add(new OrderItem
        {
            ProductId = product.Id,
            Quantity = qty,
            UnitPriceUsd = product.PriceUsd,
            UnitPriceIqd = product.PriceIqd,
            LineTotalUsd = totalUsd,
            LineTotalIqd = totalIqd
        });

        order.Payments.Add(new Payment
        {
            Provider = "WhatsApp",
            ProviderRef = $"WA-{Guid.NewGuid():N}",
            Status = "Succeeded",
            AmountUsd = totalUsd,
            AmountIqd = totalIqd,
            CreatedAt = DateTime.UtcNow
        });

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd
        });
    }

    /// <summary>
    /// إنشاء طلب من السلة (بدون واتساب). حالياً نعتبره Paid + Payment Succeeded حتى ينحسب بالإحصائيات.
    /// </summary>
    [HttpPost("cart")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckoutCart([FromBody] CheckoutCartRequest req)
    {
        if (!ValidateCheckoutSecret()) return Unauthorized();

        if (req.Items is null || req.Items.Count == 0)
            return BadRequest(new { message = "Cart is empty" });

        var userId = GetUserIdOrEmpty();

        // جلب المنتجات دفعة واحدة
        var ids = req.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id) && p.IsPublished)
            .ToDictionaryAsync(p => p.Id, p => p);

        if (products.Count == 0)
            return BadRequest(new { message = "No valid products" });

        decimal totalUsd = 0;
        decimal totalIqd = 0;

        var order = new Order
        {
            UserId = userId,
            Status = "Paid",
            TotalUsd = 0,
            TotalIqd = 0,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>(),
            Payments = new List<Payment>()
        };

        foreach (var it in req.Items)
        {
            if (!products.TryGetValue(it.ProductId, out var p)) continue;

            var qty = Math.Max(1, it.Quantity);
            var lineUsd = p.PriceUsd * qty;
            var lineIqd = p.PriceIqd * qty;

            totalUsd += lineUsd;
            totalIqd += lineIqd;

            order.Items.Add(new OrderItem
            {
                ProductId = p.Id,
                Quantity = qty,
                UnitPriceUsd = p.PriceUsd,
                UnitPriceIqd = p.PriceIqd,
                LineTotalUsd = lineUsd,
                LineTotalIqd = lineIqd
            });
        }

        if (order.Items.Count == 0)
            return BadRequest(new { message = "No valid items" });

        order.TotalUsd = totalUsd;
        order.TotalIqd = totalIqd;

        order.Payments.Add(new Payment
        {
            Provider = "WhatsApp",
            ProviderRef = $"WA-{Guid.NewGuid():N}",
            Status = "Succeeded",
            AmountUsd = totalUsd,
            AmountIqd = totalIqd,
            CreatedAt = DateTime.UtcNow
        });

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd,
            items = order.Items.Select(x => new { x.ProductId, x.Quantity, unitPriceIqd = x.UnitPriceIqd, lineTotalIqd = x.LineTotalIqd }).ToList()
        });
    }

    /// <summary>
    /// شراء عبر واتساب: ينشئ Order + Payment Succeeded ثم يعيد رابط واتساب جاهز بالتفاصيل.
    /// </summary>
    [HttpPost("cart/whatsapp")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckoutCartWhatsApp([FromBody] CheckoutWhatsAppRequest req)
    {
        if (!ValidateCheckoutSecret()) return Unauthorized();

        if (req.Items is null || req.Items.Count == 0)
            return BadRequest(new { message = "Cart is empty" });

        var userId = GetUserIdOrEmpty();

        var ids = req.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id) && p.IsPublished)
            .ToDictionaryAsync(p => p.Id, p => p);

        decimal totalUsd = 0;
        decimal totalIqd = 0;

        var order = new Order
        {
            UserId = userId,
            Status = "Paid",
            TotalUsd = 0,
            TotalIqd = 0,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>(),
            Payments = new List<Payment>()
        };

        var lines = new List<string>();

        foreach (var it in req.Items)
        {
            if (!products.TryGetValue(it.ProductId, out var p)) continue;

            var qty = Math.Max(1, it.Quantity);
            var lineUsd = p.PriceUsd * qty;
            var lineIqd = p.PriceIqd * qty;

            totalUsd += lineUsd;
            totalIqd += lineIqd;

            order.Items.Add(new OrderItem
            {
                ProductId = p.Id,
                Quantity = qty,
                UnitPriceUsd = p.PriceUsd,
                UnitPriceIqd = p.PriceIqd,
                LineTotalUsd = lineUsd,
                LineTotalIqd = lineIqd
            });

            lines.Add($"- {p.Title} x{qty} = {lineIqd:N0} د.ع");
        }

        if (order.Items.Count == 0)
            return BadRequest(new { message = "No valid items" });

        order.TotalUsd = totalUsd;
        order.TotalIqd = totalIqd;

        order.Payments.Add(new Payment
        {
            Provider = "WhatsApp",
            ProviderRef = $"WA-{Guid.NewGuid():N}",
            Status = "Succeeded",
            AmountUsd = totalUsd,
            AmountIqd = totalIqd,
            CreatedAt = DateTime.UtcNow
        });

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var phone = string.IsNullOrWhiteSpace(req.WhatsAppNumber)
            ? (_config["WHATSAPP_NUMBER"] ?? _config["WhatsApp:Number"] ?? "")
            : req.WhatsAppNumber;

        // رسالة واتساب (تُرسل معلومات العنوان/الهاتف/الملاحظات هنا فقط)
        var msg =
            "طلب جديد" + "\n" +
            $"رقم الطلب: {order.Id}" + "\n" +
            (string.IsNullOrWhiteSpace(req.PhoneNumber) ? "" : $"هاتف الزبون: {req.PhoneNumber}\n") +
            (string.IsNullOrWhiteSpace(req.ShippingAddress) ? "" : $"العنوان: {req.ShippingAddress}\n") +
            (string.IsNullOrWhiteSpace(req.Notes) ? "" : $"ملاحظات: {req.Notes}\n") +
            "\n" +
            string.Join("\n", lines) + "\n\n" +
            $"الإجمالي: {totalIqd:N0} د.ع";

        var encoded = Uri.EscapeDataString(msg);
        var waUrl = string.IsNullOrWhiteSpace(phone)
            ? $"https://wa.me/?text={encoded}"
            : $"https://wa.me/{phone}?text={encoded}";

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd,
            whatsappUrl = waUrl
        });
    }
}

public class CheckoutProductRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CheckoutCartRequest
{
    public List<CheckoutCartItem> Items { get; set; } = new();
}

public class CheckoutCartItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CheckoutWhatsAppRequest
{
    public string? WhatsAppNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public List<CheckoutCartItem> Items { get; set; } = new();
}
