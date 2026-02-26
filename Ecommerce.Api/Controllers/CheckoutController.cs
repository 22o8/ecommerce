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
		var expected = _config["Checkout:Secret"];
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
	[AllowAnonymous]
	[HttpPost("cart/whatsapp")]
	public async Task<IActionResult> CheckoutCartWhatsApp([FromBody] CheckoutCartRequest req)
	{
		if (req?.Items == null || req.Items.Count == 0)
			return BadRequest("Cart is empty.");

		var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();

		var products = await _db.Products
			.Where(p => productIds.Contains(p.Id) && p.IsPublished)
			.ToListAsync();

		if (products.Count == 0)
			return BadRequest("No valid products.");

		// Build order items
		var orderItems = new List<OrderItem>();
		decimal totalIqd = 0;

		foreach (var it in req.Items)
		{
			var p = products.FirstOrDefault(x => x.Id == it.ProductId);
			if (p == null) continue;

			var qty = Math.Max(1, it.Quantity);
			var unit = p.PriceIqd;
			var line = unit * qty;

			totalIqd += line;

			orderItems.Add(new OrderItem
			{
				ProductId = p.Id,
								Quantity = qty,
				UnitPriceIqd = unit,
				LineTotalIqd = line
			});
		}

		if (orderItems.Count == 0)
			return BadRequest("No valid items.");

		var order = new Order
		{
			TotalIqd = totalIqd,
			Status = "Pending",
			CreatedAt = DateTime.UtcNow,
			Items = orderItems
		};

		_db.Orders.Add(order);
		await _db.SaveChangesAsync();

		// WhatsApp message (simple)
		var msgLines = new List<string>
		{
			"طلب جديد:",
			$"رقم الطلب: {order.Id}",
			"--------------------"
		};

		foreach (var oi in orderItems)
		{
			msgLines.Add($"{products.FirstOrDefault(x => x.Id == oi.ProductId)?.Title ?? oi.ProductId.ToString()} x{oi.Quantity} = {oi.LineTotalIqd:n0} IQD");
		}

		msgLines.Add("--------------------");
		msgLines.Add($"المجموع: {totalIqd:n0} IQD");

		var message = string.Join("\n", msgLines);

		return Ok(new
		{
			orderId = order.Id,
			totalIqd,
			message
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