using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public class AdminCouponsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminCouponsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Coupons.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).ToListAsync();
        return Ok(items.Select(x => new { x.Id, x.Code, x.Title, x.DiscountPercent, x.FixedDiscountIqd, x.MinimumOrderIqd, x.MaxUses, x.UsedCount, x.IsActive, x.StartsAtUtc, x.EndsAtUtc }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CouponRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var code = NormalizeCode(req.Code);
        if (await _db.Coupons.AnyAsync(x => x.Code == code)) return BadRequest(new { message = "Coupon already exists" });
        var item = new Coupon
        {
            Code = code,
            Title = req.Title.Trim(),
            DiscountPercent = Math.Clamp(req.DiscountPercent, 0, 100),
            FixedDiscountIqd = Math.Max(0, req.FixedDiscountIqd),
            MinimumOrderIqd = Math.Max(0, req.MinimumOrderIqd),
            MaxUses = req.MaxUses is > 0 ? req.MaxUses : null,
            IsActive = req.IsActive,
            StartsAtUtc = req.StartsAtUtc,
            EndsAtUtc = req.EndsAtUtc
        };
        _db.Coupons.Add(item);
        await _db.SaveChangesAsync();
        return Ok(new { item.Id, item.Code });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CouponRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var item = await _db.Coupons.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        var code = NormalizeCode(req.Code);
        if (await _db.Coupons.AnyAsync(x => x.Id != id && x.Code == code)) return BadRequest(new { message = "Coupon already exists" });
        item.Code = code;
        item.Title = req.Title.Trim();
        item.DiscountPercent = Math.Clamp(req.DiscountPercent, 0, 100);
        item.FixedDiscountIqd = Math.Max(0, req.FixedDiscountIqd);
        item.MinimumOrderIqd = Math.Max(0, req.MinimumOrderIqd);
        item.MaxUses = req.MaxUses is > 0 ? req.MaxUses : null;
        item.IsActive = req.IsActive;
        item.StartsAtUtc = req.StartsAtUtc;
        item.EndsAtUtc = req.EndsAtUtc;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _db.Coupons.FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        _db.Coupons.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Deleted" });
    }

    private static string NormalizeCode(string value) => (value ?? string.Empty).Trim().ToUpperInvariant();
}

public class CouponRequest
{
    [Required, MaxLength(80)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(160)] public string Title { get; set; } = string.Empty;
    [Range(0,100)] public int DiscountPercent { get; set; }
    public decimal FixedDiscountIqd { get; set; }
    public decimal MinimumOrderIqd { get; set; }
    public int? MaxUses { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
}
