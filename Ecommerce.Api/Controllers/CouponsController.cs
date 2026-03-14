using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CouponsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromQuery] string code, [FromQuery] decimal subtotalIqd = 0)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized)) return BadRequest(new { message = "Coupon code is required" });

        var coupon = await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Code == normalized);
        if (coupon == null || !coupon.IsActive) return NotFound(new { message = "Coupon not found" });

        var now = DateTime.UtcNow;
        if (coupon.StartsAtUtc.HasValue && coupon.StartsAtUtc.Value > now)
            return BadRequest(new { message = "Coupon is not active yet" });
        if (coupon.EndsAtUtc.HasValue && coupon.EndsAtUtc.Value < now)
            return BadRequest(new { message = "Coupon expired" });
        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses.Value)
            return BadRequest(new { message = "Coupon usage limit reached" });
        if (subtotalIqd < coupon.MinimumOrderIqd)
            return BadRequest(new { message = "Minimum order not reached", minimumOrderIqd = coupon.MinimumOrderIqd });

        var percentDiscount = coupon.DiscountPercent > 0 ? Math.Round(subtotalIqd * coupon.DiscountPercent / 100m, 2) : 0m;
        var fixedDiscount = coupon.FixedDiscountIqd > 0 ? coupon.FixedDiscountIqd : 0m;
        var discountAmountIqd = Math.Min(subtotalIqd, Math.Max(percentDiscount, fixedDiscount));
        var totalIqd = Math.Max(0m, subtotalIqd - discountAmountIqd);

        return Ok(new
        {
            valid = true,
            code = coupon.Code,
            title = coupon.Title,
            discountPercent = coupon.DiscountPercent,
            fixedDiscountIqd = coupon.FixedDiscountIqd,
            discountAmountIqd,
            totalIqd,
            minimumOrderIqd = coupon.MinimumOrderIqd
        });
    }
}
