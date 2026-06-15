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
}
