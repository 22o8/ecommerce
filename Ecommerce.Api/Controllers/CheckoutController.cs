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
    private readonly IConfiguration _config;

    public CheckoutController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private bool ValidateCheckoutSecret()
    {
        var expected = _config["Checkout:Secret"] ?? _config["CHECKOUT_SECRET"] ?? Environment.GetEnvironmentVariable("CHECKOUT_SECRET");
        if (string.IsNullOrWhiteSpace(expected)) return true;
        var provided = Request.Headers["X-Checkout-Secret"].ToString();
        if (!string.IsNullOrWhiteSpace(provided) && provided == expected) return true;
        return User?.Identity?.IsAuthenticated == true;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idStr, out userId);
    }

    private async Task<(Coupon? coupon, decimal discountIqd, decimal discountUsd, IActionResult? error)> ResolveCouponAsync(string? code, decimal subtotalIqd, decimal subtotalUsd, Guid? userId = null, string? deviceKey = null, IEnumerable<Product>? products = null)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized)) return (null, 0m, 0m, null);

        var coupon = await _db.Coupons.FirstOrDefaultAsync(x => x.Code == normalized);
        if (coupon == null || !coupon.IsActive)
            return (null, 0m, 0m, BadRequest(new { message = "Coupon is invalid" }));

        var now = DateTime.UtcNow;
        if (coupon.StartsAtUtc.HasValue && coupon.StartsAtUtc.Value > now)
            return (null, 0m, 0m, BadRequest(new { message = "Coupon is not active yet" }));
        if (coupon.EndsAtUtc.HasValue && coupon.EndsAtUtc.Value < now)
            return (null, 0m, 0m, BadRequest(new { message = "Coupon expired" }));
        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
            return (null, 0m, 0m, BadRequest(new { message = "Coupon usage limit reached" }));
        if (subtotalIqd < coupon.MinimumOrderIqd)
            return (null, 0m, 0m, BadRequest(new { message = "Minimum order not reached", minimumOrderIqd = coupon.MinimumOrderIqd }));

        if (products != null && products.Any(p => !p.IsCouponAllowed))
            return (null, 0m, 0m, BadRequest(new { message = "Coupon cannot be applied to one or more products" }));

        var deviceHash = CouponsController.HashDeviceKey(deviceKey);
        if (!string.IsNullOrWhiteSpace(deviceHash))
        {
            var usedByDevice = await _db.CouponUsages.AnyAsync(x => x.CouponId == coupon.Id && x.DeviceKeyHash == deviceHash);
            if (usedByDevice)
                return (null, 0m, 0m, BadRequest(new { message = "Coupon already used on this device" }));
        }
        if (userId.HasValue)
        {
            var usedByUser = await _db.CouponUsages.AnyAsync(x => x.CouponId == coupon.Id && x.UserId == userId.Value);
            if (usedByUser)
                return (null, 0m, 0m, BadRequest(new { message = "Coupon already used by this account" }));
        }

        var percentDiscountIqd = coupon.DiscountPercent > 0 ? Math.Round(subtotalIqd * coupon.DiscountPercent / 100m, 2) : 0m;
        var fixedDiscountIqd = coupon.FixedDiscountIqd > 0 ? coupon.FixedDiscountIqd : 0m;
        var discountIqd = Math.Min(subtotalIqd, Math.Max(percentDiscountIqd, fixedDiscountIqd));

        var discountUsd = subtotalIqd > 0m ? Math.Round(subtotalUsd * (discountIqd / subtotalIqd), 2) : 0m;
        return (coupon, discountIqd, discountUsd, null);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CheckoutProduct([FromBody] CheckoutProductRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId && p.IsPublished);
        if (product == null) return NotFound("Product not found");

        var qty = req.Quantity <= 0 ? 1 : req.Quantity;
        if (qty > 50) return BadRequest("Quantity too large");
        if (product.StockQuantity < qty) return BadRequest(new { message = "Insufficient stock", available = product.StockQuantity });

        var subtotalUsd = product.PriceUsd * qty;
        var subtotalIqd = product.PriceIqd * qty;
        var couponResult = await ResolveCouponAsync(req.CouponCode, subtotalIqd, subtotalUsd, userId, req.DeviceKey, products);
        if (couponResult.error != null) return couponResult.error;

        var order = new Order
        {
            UserId = userId,
            Status = "PendingPayment",
            SubtotalUsd = subtotalUsd,
            SubtotalIqd = subtotalIqd,
            DiscountAmountUsd = couponResult.discountUsd,
            DiscountAmountIqd = couponResult.discountIqd,
            CouponCode = couponResult.coupon?.Code,
            TotalUsd = Math.Max(0m, subtotalUsd - couponResult.discountUsd),
            TotalIqd = Math.Max(0m, subtotalIqd - couponResult.discountIqd)
        };

        order.Items.Add(new OrderItem
        {
            ItemType = "DigitalProduct",
            ProductId = product.Id,
            UnitPriceUsd = product.PriceUsd,
            Quantity = qty,
            LineTotalUsd = subtotalUsd,
            UnitPriceIqd = product.PriceIqd,
            LineTotalIqd = subtotalIqd
        });

        var payment = new Payment
        {
            Order = order,
            Provider = "Mock",
            Status = "Pending",
            ProviderRef = $"MOCK-{Guid.NewGuid():N}",
            AmountUsd = order.TotalUsd,
            AmountIqd = order.TotalIqd
        };

        order.Payments.Add(payment);
        product.StockQuantity = Math.Max(0, product.StockQuantity - qty);
        if (couponResult.coupon != null)
        {
            couponResult.coupon.UsedCount += 1;
            _db.CouponUsages.Add(new CouponUsage { CouponId = couponResult.coupon.Id, UserId = userId, OrderId = order.Id, DeviceKeyHash = CouponsController.HashDeviceKey(req.DeviceKey) });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            subtotalIqd = order.SubtotalIqd,
            discountAmountIqd = order.DiscountAmountIqd,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd,
            couponCode = order.CouponCode,
            payment = new { payment.Id, payment.Provider, payment.Status, payment.ProviderRef }
        });
    }

    [HttpPost("cart")]
    public async Task<IActionResult> CheckoutCart([FromBody] CheckoutCartRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");
        if (req?.Items == null || req.Items.Count == 0) return BadRequest("Cart is empty");
        if (req.Items.Count > 50) return BadRequest("Too many items");

        var items = req.Items.Where(i => i.ProductId != Guid.Empty).Select(i => new { i.ProductId, Quantity = i.Quantity <= 0 ? 1 : i.Quantity }).ToList();
        if (items.Count == 0) return BadRequest("Cart is empty");
        if (items.Any(i => i.Quantity > 50)) return BadRequest("Quantity too large");

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id) && p.IsPublished).ToListAsync();
        if (products.Count != productIds.Count) return NotFound("One or more products not found");

        var insufficient = items.Select(i => new { item = i, product = products.First(x => x.Id == i.ProductId) }).FirstOrDefault(x => x.product.StockQuantity < x.item.Quantity);
        if (insufficient != null) return BadRequest(new { message = "Insufficient stock", productId = insufficient.product.Id, available = insufficient.product.StockQuantity });

        var order = new Order { UserId = userId, Status = "PendingPayment", TotalUsd = 0, TotalIqd = 0 };
        decimal subtotalUsd = 0m;
        decimal subtotalIqd = 0m;

        foreach (var i in items)
        {
            var p = products.First(x => x.Id == i.ProductId);
            var lineTotalUsd = p.PriceUsd * i.Quantity;
            var lineTotalIqd = p.PriceIqd * i.Quantity;
            subtotalUsd += lineTotalUsd;
            subtotalIqd += lineTotalIqd;

            order.Items.Add(new OrderItem
            {
                ItemType = "DigitalProduct",
                ProductId = p.Id,
                UnitPriceUsd = p.PriceUsd,
                Quantity = i.Quantity,
                LineTotalUsd = lineTotalUsd,
                UnitPriceIqd = p.PriceIqd,
                LineTotalIqd = lineTotalIqd
            });
        }

        var couponResult = await ResolveCouponAsync(req.CouponCode, subtotalIqd, subtotalUsd, userId, req.DeviceKey, products);
        if (couponResult.error != null) return couponResult.error;

        order.SubtotalUsd = subtotalUsd;
        order.SubtotalIqd = subtotalIqd;
        order.DiscountAmountUsd = couponResult.discountUsd;
        order.DiscountAmountIqd = couponResult.discountIqd;
        order.CouponCode = couponResult.coupon?.Code;
        order.TotalUsd = Math.Max(0m, subtotalUsd - couponResult.discountUsd);
        order.TotalIqd = Math.Max(0m, subtotalIqd - couponResult.discountIqd);

        foreach (var i in items)
        {
            var p = products.First(x => x.Id == i.ProductId);
            p.StockQuantity = Math.Max(0, p.StockQuantity - i.Quantity);
        }

        var payment = new Payment
        {
            Order = order,
            Provider = "Mock",
            Status = "Pending",
            ProviderRef = $"MOCK-{Guid.NewGuid():N}",
            AmountUsd = order.TotalUsd,
            AmountIqd = order.TotalIqd
        };

        order.Payments.Add(payment);
        if (couponResult.coupon != null)
        {
            couponResult.coupon.UsedCount += 1;
            _db.CouponUsages.Add(new CouponUsage { CouponId = couponResult.coupon.Id, UserId = userId, OrderId = order.Id, DeviceKeyHash = CouponsController.HashDeviceKey(req.DeviceKey) });
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            subtotalIqd = order.SubtotalIqd,
            discountAmountIqd = order.DiscountAmountIqd,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd,
            couponCode = order.CouponCode,
            items = order.Items.Select(x => new { x.ProductId, x.Quantity, x.UnitPriceIqd, x.LineTotalIqd, x.UnitPriceUsd, x.LineTotalUsd }).ToList(),
            payment = new { payment.Id, payment.Provider, payment.Status, payment.ProviderRef }
        });
    }

    [Authorize]
    [HttpPost("cart/whatsapp")]
    public async Task<IActionResult> CheckoutCartWhatsApp([FromBody] CheckoutCartRequest req)
    {
        if (req == null || req.Items == null || req.Items.Count == 0)
            return BadRequest(new { message = "Cart is empty" });
        if (!ValidateCheckoutSecret())
            return Unauthorized(new { message = "Invalid checkout secret" });
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "Unauthorized" });

        req.Items = req.Items.Where(i => i.ProductId != Guid.Empty).Select(i => new CheckoutCartItem { ProductId = i.ProductId, Quantity = Math.Max(1, i.Quantity) }).ToList();
        if (req.Items.Count == 0) return BadRequest(new { message = "Cart is empty" });

        var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Include(p => p.Images).Where(p => productIds.Contains(p.Id)).ToListAsync();
        if (products.Count == 0) return BadRequest(new { message = "Cart is empty" });

        var insufficient = req.Items.Select(i => new { item = i, product = products.FirstOrDefault(x => x.Id == i.ProductId) }).FirstOrDefault(x => x.product != null && x.product.StockQuantity < x.item.Quantity);
        if (insufficient != null) return BadRequest(new { message = "Insufficient stock", productId = insufficient.product!.Id, available = insufficient.product.StockQuantity });

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Paid",
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>(),
            Payments = new List<Payment>()
        };

        decimal subtotalIqd = 0, subtotalUsd = 0;
        foreach (var i in req.Items)
        {
            var p = products.FirstOrDefault(x => x.Id == i.ProductId);
            if (p == null) continue;
            var lineTotalUsd = p.PriceUsd * i.Quantity;
            var lineTotalIqd = p.PriceIqd * i.Quantity;
            subtotalUsd += lineTotalUsd;
            subtotalIqd += lineTotalIqd;
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ItemType = "Product",
                ProductId = p.Id,
                Quantity = i.Quantity,
                UnitPriceUsd = p.PriceUsd,
                LineTotalUsd = lineTotalUsd,
                UnitPriceIqd = p.PriceIqd,
                LineTotalIqd = lineTotalIqd
            });
            p.StockQuantity = Math.Max(0, p.StockQuantity - i.Quantity);
        }

        var couponResult = await ResolveCouponAsync(req.CouponCode, subtotalIqd, subtotalUsd, userId, req.DeviceKey, products);
        if (couponResult.error != null) return couponResult.error;

        order.SubtotalUsd = subtotalUsd;
        order.SubtotalIqd = subtotalIqd;
        order.DiscountAmountUsd = couponResult.discountUsd;
        order.DiscountAmountIqd = couponResult.discountIqd;
        order.CouponCode = couponResult.coupon?.Code;
        order.TotalUsd = Math.Max(0m, subtotalUsd - couponResult.discountUsd);
        order.TotalIqd = Math.Max(0m, subtotalIqd - couponResult.discountIqd);

        var payment = new Payment
        {
            Order = order,
            Provider = "WhatsApp",
            Status = "Succeeded",
            ProviderRef = $"WA-{Guid.NewGuid():N}",
            AmountUsd = order.TotalUsd,
            AmountIqd = order.TotalIqd
        };

        order.Payments.Add(payment);
        if (couponResult.coupon != null)
        {
            couponResult.coupon.UsedCount += 1;
            _db.CouponUsages.Add(new CouponUsage { CouponId = couponResult.coupon.Id, UserId = userId, OrderId = order.Id, DeviceKeyHash = CouponsController.HashDeviceKey(req.DeviceKey) });
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new { orderId = order.Id, status = order.Status, couponCode = order.CouponCode, discountAmountIqd = order.DiscountAmountIqd, amountIqd = order.TotalIqd });
    }

    [HttpPost("services")]
    public async Task<IActionResult> CheckoutService([FromBody] CheckoutServiceRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var sr = await _db.ServiceRequests.Include(x => x.Package).Include(x => x.Service).FirstOrDefaultAsync(x => x.Id == req.ServiceRequestId && x.UserId == userId);
        if (sr == null) return NotFound("ServiceRequest not found");
        if (sr.Status != "PendingPayment") return BadRequest("ServiceRequest is not pending payment");

        var priceUsd = sr.Package.PriceUsd;
        var priceIqd = sr.Package.PriceIqd > 0 ? sr.Package.PriceIqd : sr.Package.PriceUsd;

        var order = new Order
        {
            UserId = userId,
            Status = "PendingPayment",
            SubtotalUsd = priceUsd,
            SubtotalIqd = priceIqd,
            TotalUsd = priceUsd,
            TotalIqd = priceIqd
        };

        order.Items.Add(new OrderItem
        {
            ItemType = "Service",
            ServiceId = sr.ServiceId,
            PackageId = sr.PackageId,
            ServiceRequestId = sr.Id,
            UnitPriceUsd = priceUsd,
            Quantity = 1,
            LineTotalUsd = priceUsd,
            UnitPriceIqd = priceIqd,
            LineTotalIqd = priceIqd
        });

        var payment = new Payment
        {
            Order = order,
            Provider = "Mock",
            Status = "Pending",
            ProviderRef = $"MOCK-{Guid.NewGuid():N}",
            AmountUsd = order.TotalUsd,
            AmountIqd = order.TotalIqd
        };

        order.Payments.Add(payment);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new { orderId = order.Id, status = order.Status, amountUsd = order.TotalUsd, amountIqd = order.TotalIqd, service = new { sr.Id, sr.Service.Title, package = sr.Package.Name }, payment = new { payment.Id, payment.Provider, payment.Status, payment.ProviderRef } });
    }
}

public class CheckoutProductRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? CouponCode { get; set; }
    public string? DeviceKey { get; set; }
}

public class CheckoutCartRequest
{
    public List<CheckoutCartItem> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? DeviceKey { get; set; }
}

public class CheckoutCartItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CheckoutServiceRequest
{
    public Guid ServiceRequestId { get; set; }
}
