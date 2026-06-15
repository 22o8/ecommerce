using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/loyalty")]
[Authorize(Roles = "Admin")]
public class AdminLoyaltyController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminLoyaltyController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var users = await _db.Users.AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new {
                u.Id, u.FullName, u.Email, u.Phone, u.ReferralCode, u.CreatedAt,
                points = _db.PointsWallets.Where(w => w.UserId == u.Id).Select(w => (int?)w.Balance).FirstOrDefault() ?? 0,
                referrals = _db.Referrals.Count(r => r.ReferrerUserId == u.Id)
            })
            .Take(200)
            .ToListAsync();

        var leaderboard = await _db.Users.AsNoTracking()
            .Select(u => new {
                u.Id, u.FullName, u.Email, u.Phone, u.ReferralCode,
                referrals = _db.Referrals.Count(r => r.ReferrerUserId == u.Id),
                points = _db.PointsWallets.Where(w => w.UserId == u.Id).Select(w => (int?)w.Balance).FirstOrDefault() ?? 0
            })
            .OrderByDescending(x => x.referrals)
            .ThenByDescending(x => x.points)
            .Take(50)
            .ToListAsync();

        var referrals = await _db.Referrals.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(200)
            .Select(x => new {
                x.Id, x.ReferralCode, x.Status, x.Rewarded, x.RewardPoints, x.RewardCouponCode, x.CreatedAtUtc, x.RewardedAtUtc,
                referrer = new { x.ReferrerUserId, x.ReferrerUser!.FullName, x.ReferrerUser.Email, x.ReferrerUser.Phone },
                referred = new { x.ReferredUserId, x.ReferredUser!.FullName, x.ReferredUser.Email, x.ReferredUser.Phone }
            }).ToListAsync();

        return Ok(new { users, leaderboard, referrals });
    }

    public record GrantPointsRequest(Guid UserId, int Points, string? Note, Guid? ReferralId = null);

    [HttpPost("grant-points")]
    public async Task<IActionResult> GrantPoints([FromBody] GrantPointsRequest req)
    {
        if (req.UserId == Guid.Empty || req.Points <= 0) return BadRequest(new { message = "User and positive points are required." });
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == req.UserId);
        if (user == null) return NotFound(new { message = "User not found." });
        var wallet = await _db.PointsWallets.FirstOrDefaultAsync(x => x.UserId == req.UserId);
        if (wallet == null)
        {
            wallet = new PointsWallet { UserId = req.UserId };
            _db.PointsWallets.Add(wallet);
        }
        wallet.Balance += req.Points;
        wallet.LifetimeEarned += req.Points;
        wallet.UpdatedAtUtc = DateTime.UtcNow;
        _db.PointsTransactions.Add(new PointsTransaction { Wallet = wallet, UserId = req.UserId, Points = req.Points, Type = req.ReferralId.HasValue ? "ReferralGift" : "Manual", Note = req.Note ?? "هدية نقاط من الإدارة" });
        _db.UserGifts.Add(new UserGift { UserId = req.UserId, ReferralId = req.ReferralId, GiftType = "Points", Points = req.Points, Title = "هدية نقاط", Message = req.Note ?? $"تمت إضافة {req.Points} نقطة إلى محفظتك." });
        if (req.ReferralId.HasValue)
        {
            var referral = await _db.Referrals.FirstOrDefaultAsync(x => x.Id == req.ReferralId.Value);
            if (referral != null)
            {
                referral.Rewarded = true;
                referral.Status = "Rewarded";
                referral.RewardType = "Points";
                referral.RewardPoints = req.Points;
                referral.RewardedAtUtc = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync();
        return Ok(new { message = "Points granted", userId = req.UserId, points = req.Points, balance = wallet.Balance });
    }

    public record GrantCouponRequest(Guid UserId, string CouponCode, string? Note, Guid? ReferralId = null);

    [HttpPost("grant-coupon")]
    public async Task<IActionResult> GrantCoupon([FromBody] GrantCouponRequest req)
    {
        if (req.UserId == Guid.Empty || string.IsNullOrWhiteSpace(req.CouponCode)) return BadRequest(new { message = "User and coupon are required." });
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == req.UserId);
        if (user == null) return NotFound(new { message = "User not found." });
        _db.UserGifts.Add(new UserGift { UserId = req.UserId, ReferralId = req.ReferralId, GiftType = "Coupon", CouponCode = req.CouponCode.Trim().ToUpperInvariant(), Title = "كوبون هدية", Message = req.Note ?? $"وصلتك هدية كوبون: {req.CouponCode}" });
        if (req.ReferralId.HasValue)
        {
            var referral = await _db.Referrals.FirstOrDefaultAsync(x => x.Id == req.ReferralId.Value);
            if (referral != null)
            {
                referral.Rewarded = true;
                referral.Status = "Rewarded";
                referral.RewardType = "Coupon";
                referral.RewardCouponCode = req.CouponCode.Trim().ToUpperInvariant();
                referral.RewardedAtUtc = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync();
        return Ok(new { message = "Coupon gift sent", userId = req.UserId, couponCode = req.CouponCode });
    }
}
