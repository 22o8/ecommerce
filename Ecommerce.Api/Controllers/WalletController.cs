using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _db;
    public WalletController(AppDbContext db) => _db = db;

    private bool TryGetUserId(out Guid userId)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idStr, out userId);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) return NotFound();
        var wallet = await _db.PointsWallets.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
        var transactions = await _db.PointsTransactions.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(30)
            .Select(x => new { x.Id, x.Type, x.Points, x.Note, x.OrderId, x.CreatedAtUtc })
            .ToListAsync();
        var gifts = await _db.UserGifts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .Select(x => new { x.Id, x.GiftType, x.Points, x.CouponCode, x.Title, x.Message, x.IsRead, x.CreatedAtUtc })
            .ToListAsync();
        var referralCount = await _db.Referrals.CountAsync(x => x.ReferrerUserId == userId);
        return Ok(new {
            balance = wallet?.Balance ?? 0,
            lifetimeEarned = wallet?.LifetimeEarned ?? 0,
            lifetimeSpent = wallet?.LifetimeSpent ?? 0,
            referralCode = user.ReferralCode,
            referralUrl = $"https://drseoulbeauty.store/register?ref={user.ReferralCode}",
            referralCount,
            transactions,
            gifts
        });
    }

    [HttpGet("notifications/unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var count = await _db.UserGifts.AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead, ct);
        return Ok(new { count });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications([FromQuery] int take = 30, CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        take = Math.Clamp(take, 1, 100);
        var gifts = await _db.UserGifts.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .Select(x => new { x.Id, x.GiftType, x.Points, x.CouponCode, x.Title, x.Message, x.IsRead, x.CreatedAtUtc })
            .ToListAsync(ct);
        var unread = gifts.Count(x => !x.IsRead);
        return Ok(new { unread, items = gifts });
    }

    [HttpPost("notifications/{id:guid}/read")]
    public async Task<IActionResult> MarkNotificationRead(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var gift = await _db.UserGifts.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (gift == null) return NotFound();
        if (!gift.IsRead)
        {
            gift.IsRead = true;
            await _db.SaveChangesAsync(ct);
        }
        return Ok(new { ok = true });
    }

    [HttpPost("notifications/read-all")]
    public async Task<IActionResult> MarkAllNotificationsRead(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var gifts = await _db.UserGifts
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(ct);
        foreach (var gift in gifts) gift.IsRead = true;
        if (gifts.Count > 0) await _db.SaveChangesAsync(ct);
        return Ok(new { ok = true, updated = gifts.Count });
    }

}
