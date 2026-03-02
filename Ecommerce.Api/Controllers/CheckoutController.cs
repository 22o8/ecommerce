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
		return !string.IsNullOrWhiteSpace(provided) && provided == expected;
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
            TotalUsd = product.PriceUsd * qty,
            TotalIqd = product.PriceIqd * qty
        };

        order.Items.Add(new OrderItem
        {
            ItemType = "DigitalProduct",
            ProductId = product.Id,
            UnitPriceUsd = product.PriceUsd,
            Quantity = qty,
            LineTotalUsd = product.PriceUsd * qty,
            UnitPriceIqd = product.PriceIqd,
            LineTotalIqd = product.PriceIqd * qty
        });

        // إنشاء Payment (Mock)
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

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd,
            payment = new { payment.Id, payment.Provider, payment.Status, payment.ProviderRef }
        });
    }

    // POST /api/checkout/cart
    // ينشئ Order واحد لعدة منتجات من السلة
    [HttpPost("cart")]
    public async Task<IActionResult> CheckoutCart([FromBody] CheckoutCartRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        if (req?.Items == null || req.Items.Count == 0)
            return BadRequest("Cart is empty");

        if (req.Items.Count > 50)
            return BadRequest("Too many items");

        // sanitize quantities and collect ids
        var items = req.Items
            .Where(i => i.ProductId != Guid.Empty)
            .Select(i => new { i.ProductId, Quantity = i.Quantity <= 0 ? 1 : i.Quantity })
            .ToList();

        if (items.Count == 0) return BadRequest("Cart is empty");

        if (items.Any(i => i.Quantity > 50))
            return BadRequest("Quantity too large");

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsPublished)
            .ToListAsync();

        if (products.Count != productIds.Count)
            return NotFound("One or more products not found");

        var order = new Order
        {
            UserId = userId,
            Status = "PendingPayment",
            TotalUsd = 0,
            TotalIqd = 0
        };

        foreach (var i in items)
        {
            var p = products.First(x => x.Id == i.ProductId);
            var lineTotalUsd = p.PriceUsd * i.Quantity;
            var lineTotalIqd = p.PriceIqd * i.Quantity;
            order.TotalUsd += lineTotalUsd;
            order.TotalIqd += lineTotalIqd;

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

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd,
            items = order.Items.Select(x => new { x.ProductId, x.Quantity, x.UnitPriceIqd, x.LineTotalIqd, x.UnitPriceUsd, x.LineTotalUsd }).ToList(),
            payment = new { payment.Id, payment.Provider, payment.Status, payment.ProviderRef }
        });
    }

	// POST /api/checkout/cart/whatsapp
	// Checkout عبر واتساب (بدون بوابة دفع): نسجل الطلب كـ مدفوع + نسجل Payment ناجح حتى يظهر بالإحصائيات
	// ملاحظة أمنية: فعّل Checkout:Secret وأرسل X-Checkout-Secret من الـ BFF فقط.
	// مهم: لازم المستخدم يكون مسجل دخول حتى ما نخسر قيود FK على UserId.
	[Authorize]
	[HttpPost("cart/whatsapp")]
	public async Task<IActionResult> CheckoutCartWhatsApp([FromBody] CheckoutCartRequest req)
	{
		if (req == null || req.Items == null || req.Items.Count == 0)
			return BadRequest(new { message = "Cart is empty" });

		if (!ValidateCheckoutSecret())
			return Unauthorized(new { message = "Invalid checkout secret" });

		// لازم يكون المستخدم مسجل دخول ونقدر نستخرج الـ UserId من الـ JWT
		if (!TryGetUserId(out var userId))
			return Unauthorized(new { message = "Unauthorized" });

		// نفس منطق /cart: نظّف الكميات
		req.Items = req.Items
			.Where(i => i.ProductId != Guid.Empty)
			.Select(i => new CheckoutCartItem { ProductId = i.ProductId, Quantity = Math.Max(1, i.Quantity) })
			.ToList();

		if (req.Items.Count == 0)
			return BadRequest(new { message = "Cart is empty" });

		var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
		var products = await _db.Products
			.Include(p => p.Images)
			.Where(p => productIds.Contains(p.Id))
			.ToListAsync();

		if (products.Count == 0)
			return BadRequest(new { message = "Cart is empty" });

		var order = new Order
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Status = "Paid",
			CreatedAt = DateTime.UtcNow,
			Items = new List<OrderItem>(),
			Payments = new List<Payment>()
		};

		decimal totalIqd = 0;
		decimal totalUsd = 0;

		foreach (var i in req.Items)
		{
			var p = products.FirstOrDefault(x => x.Id == i.ProductId);
			if (p == null) continue;

			var lineTotalUsd = p.PriceUsd * i.Quantity;
			var lineTotalIqd = p.PriceIqd * i.Quantity;
			totalUsd += lineTotalUsd;
			totalIqd += lineTotalIqd;

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
		}

		order.TotalUsd = totalUsd;
		order.TotalIqd = totalIqd;

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
		_db.Orders.Add(order);
		await _db.SaveChangesAsync();

		return Ok(new { orderId = order.Id, status = order.Status });
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

        // ✅ نعتمد IQD بالدرجة الأولى
        var priceUsd = sr.Package.PriceUsd;
        var priceIqd = sr.Package.PriceIqd > 0 ? sr.Package.PriceIqd : sr.Package.PriceUsd;

        var order = new Order
        {
            UserId = userId,
            Status = "PendingPayment",
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

        return Ok(new
        {
            orderId = order.Id,
            status = order.Status,
            amountUsd = order.TotalUsd,
            amountIqd = order.TotalIqd,
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

public class CheckoutCartRequest
{
    public List<CheckoutCartItem> Items { get; set; } = new();
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
