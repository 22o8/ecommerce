using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminOrdersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // ✅ مهم: تجنب Include(User) لأنه يقرأ كل أعمدة Users وقد يسبب 500 إذا DB غير محدثة (مثل Phone)
        try
        {
            var orders = await _db.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.SubtotalIqd,
                o.DiscountAmountIqd,
                o.CouponCode,
                o.TotalIqd,
                    o.TotalUsd,
                    o.DeliveryFeeIqd,
                    o.CustomerNote,
                    o.AdminNote,
                    o.PointsEarned,
                    o.PointsAwarded,
                    o.CreatedAt,

                    // ✅ حقول مسطّحة لتسهيل الفرونت
                    userEmail = o.User.Email,
                    userFullName = o.User.FullName,

                    User = new
                    {
                        o.UserId,
                        FullName = o.User.FullName,
                        Email = o.User.Email
                    },

                    Items = o.Items.Select(i => new
                    {
                        i.ItemType,
                        i.Quantity,
                        i.UnitPriceIqd,
                        i.UnitPriceUsd,
                        i.LineTotalIqd,
                        i.LineTotalUsd,
                        i.ProductId,
                        productTitle = i.Product != null ? i.Product.Title : null,
                        i.ServiceId,
                        serviceTitle = i.Service != null ? i.Service.Title : null,
                        i.PackageId,
                        packageName = i.Package != null ? i.Package.Name : null,
                        i.ServiceRequestId
                    }).ToList(),

                    // ✅ عنوان سريع للعرض في الجدول (أول عنصر)
                    primaryItemTitle = o.Items
                        .Select(i =>
                            i.Product != null ? i.Product.Title :
                            (i.Service != null ? i.Service.Title :
                            (i.Package != null ? i.Package.Name : null)))
                        .FirstOrDefault(),

                    Payments = o.Payments.Select(p => new
                    {
                        p.Id,
                        p.Provider,
                        p.Status,
                        p.AmountIqd,
                        p.AmountUsd
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load orders.", detail = ex.Message });
        }
    }

    // ✅ تفاصيل طلب واحد (لا نعرض JSON خام بالفرونت)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var o = await _db.Orders
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    x.TotalIqd,
                    x.TotalUsd,
                    x.SubtotalIqd,
                    x.DiscountAmountIqd,
                    x.CouponCode,
                    x.DeliveryFeeIqd,
                    x.CustomerNote,
                    x.AdminNote,
                    x.PointsEarned,
                    x.PointsAwarded,
                    x.CreatedAt,
                    user = new
                    {
                        x.UserId,
                        fullName = x.User.FullName,
                        email = x.User.Email
                    },
                    items = x.Items.Select(i => new
                    {
                        i.Id,
                        i.ItemType,
                        i.Quantity,
                        i.UnitPriceIqd,
                        i.LineTotalIqd,
                        i.UnitPriceUsd,
                        i.LineTotalUsd,
                        i.ProductId,
                        productTitle = i.Product != null ? i.Product.Title : null,
                        i.ServiceId,
                        serviceTitle = i.Service != null ? i.Service.Title : null,
                        i.PackageId,
                        packageName = i.Package != null ? i.Package.Name : null,
                        i.ServiceRequestId
                    }).ToList(),
                    payments = x.Payments.Select(p => new
                    {
                        p.Id,
                        p.Provider,
                        p.Status,
                        p.AmountIqd,
                        p.AmountUsd,
                        p.ProviderRef,
                        p.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (o == null) return NotFound(new { message = "Order not found." });
            return Ok(o);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load order.", detail = ex.Message });
        }
    }


    public sealed record UpdateOrderStatusRequest(string Status, decimal? DeliveryFeeIqd, string? AdminNote, int? PointsOverride);

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest req)
    {
        var order = await _db.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound(new { message = "Order not found." });

        var allowed = new[] { "PendingSale", "Sold", "NotSold" };
        var status = string.IsNullOrWhiteSpace(req.Status) ? order.Status : req.Status.Trim();
        if (!allowed.Contains(status)) return BadRequest(new { message = "Invalid status." });

        if (req.DeliveryFeeIqd.HasValue)
        {
            var oldFee = order.DeliveryFeeIqd;
            var newFee = Math.Max(0m, req.DeliveryFeeIqd.Value);
            order.DeliveryFeeIqd = newFee;
            order.TotalIqd = Math.Max(0m, order.TotalIqd - oldFee + newFee);
        }
        if (req.AdminNote != null) order.AdminNote = req.AdminNote.Trim();

        order.Status = status;
        if (status == "Sold")
        {
            order.SoldAt ??= DateTime.UtcNow;
            await AwardOrderPointsAsync(order, req.PointsOverride);
        }
        if (status == "NotSold")
        {
            order.SoldAt = null;
        }

        await _db.SaveChangesAsync();
        return Ok(new { order.Id, order.Status, order.DeliveryFeeIqd, order.TotalIqd, order.AdminNote, order.PointsEarned, order.PointsAwarded });
    }

    private async Task AwardOrderPointsAsync(Order order, int? overridePoints)
    {
        if (order.PointsAwarded) return;
        var points = overridePoints.HasValue ? Math.Max(0, overridePoints.Value) : (int)Math.Floor(order.TotalIqd / 1000m);
        order.PointsEarned = points;
        if (points <= 0) return;
        var wallet = await _db.PointsWallets.FirstOrDefaultAsync(x => x.UserId == order.UserId);
        if (wallet == null)
        {
            wallet = new PointsWallet { UserId = order.UserId };
            _db.PointsWallets.Add(wallet);
        }
        wallet.Balance += points;
        wallet.LifetimeEarned += points;
        wallet.UpdatedAtUtc = DateTime.UtcNow;
        _db.PointsTransactions.Add(new PointsTransaction
        {
            Wallet = wallet,
            UserId = order.UserId,
            OrderId = order.Id,
            Type = "Purchase",
            Points = points,
            Note = $"نقاط عملية شراء للطلب {order.Id}"
        });
        _db.UserGifts.Add(new UserGift
        {
            UserId = order.UserId,
            GiftType = "Points",
            Points = points,
            Title = "نقاط شراء",
            Message = $"تمت إضافة {points} نقطة إلى محفظتك بعد تأكيد البيع."
        });
        order.PointsAwarded = true;
        order.PointsAwardedAtUtc = DateTime.UtcNow;
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound(new { message = "Order not found." });

            // مهم: توجد جداول مرتبطة بالطلب لا تحذف تلقائياً دائماً مثل CouponUsages
            // لذلك نحذفها يدويًا قبل حذف الطلب لتجنب خطأ FK و 500.
            var couponUsages = await _db.CouponUsages
                .Where(x => x.OrderId == id)
                .ToListAsync();

            var downloadTokens = await _db.DownloadTokens
                .Where(x => x.OrderId == id)
                .ToListAsync();

            var pointsTransactions = await _db.PointsTransactions
                .Where(x => x.OrderId == id)
                .ToListAsync();

            if (couponUsages.Count > 0) _db.CouponUsages.RemoveRange(couponUsages);
            if (downloadTokens.Count > 0) _db.DownloadTokens.RemoveRange(downloadTokens);
            if (pointsTransactions.Count > 0) _db.PointsTransactions.RemoveRange(pointsTransactions);
            if (order.Items.Count > 0) _db.OrderItems.RemoveRange(order.Items);
            if (order.Payments.Count > 0) _db.Payments.RemoveRange(order.Payments);

            _db.Orders.Remove(order);

            await _db.SaveChangesAsync();
            return Ok(new { message = "Order deleted.", id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to delete order.", detail = ex.Message });
        }
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .ToListAsync();

            var orderIds = orders.Select(o => o.Id).ToList();

            if (orderIds.Count > 0)
            {
                var couponUsages = await _db.CouponUsages
                    .Where(x => x.OrderId != null && orderIds.Contains(x.OrderId.Value))
                    .ToListAsync();

                var downloadTokens = await _db.DownloadTokens
                    .Where(x => orderIds.Contains(x.OrderId))
                    .ToListAsync();

                var pointsTransactions = await _db.PointsTransactions
                    .Where(x => x.OrderId != null && orderIds.Contains(x.OrderId.Value))
                    .ToListAsync();

                if (couponUsages.Count > 0) _db.CouponUsages.RemoveRange(couponUsages);
                if (downloadTokens.Count > 0) _db.DownloadTokens.RemoveRange(downloadTokens);
                if (pointsTransactions.Count > 0) _db.PointsTransactions.RemoveRange(pointsTransactions);
            }

            _db.OrderItems.RemoveRange(orders.SelectMany(o => o.Items));
            _db.Payments.RemoveRange(orders.SelectMany(o => o.Payments));
            _db.Orders.RemoveRange(orders);

            await _db.SaveChangesAsync();
            return Ok(new { message = "All orders deleted.", count = orders.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to delete all orders.", detail = ex.Message });
        }
    }
}
