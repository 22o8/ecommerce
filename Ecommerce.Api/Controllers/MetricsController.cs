using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/metrics")]
public class MetricsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MetricsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid? TryGetUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User?.FindFirst("sub")?.Value
                    ?? User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrWhiteSpace(claim) && Guid.TryParse(claim, out var parsed))
            return parsed;

        return null;
    }

    public record VisitReq(string? Path);

    // POST /api/metrics/visit
    // يناديه الفرونت عند فتح أي صفحة (مرة واحدة لكل تحميل/جلسة حسب منطق الفرونت)
    [HttpPost("visit")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackVisit([FromBody] VisitReq? body)
    {
        var path = (body?.Path ?? "").Trim();
        if (path.Length > 400) path = path[..400];

        _db.SiteVisits.Add(new SiteVisit
        {
            UserId = TryGetUserId(),
            Path = string.IsNullOrWhiteSpace(path) ? null : path
        });

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    // GET /api/metrics/visits/summary
    // عدّاد عام + اليوم + هذا الشهر (مفيد للواجهة العامة أو لوحة الأدمن)
    [HttpGet("visits/summary")]
    [AllowAnonymous]
    public async Task<IActionResult> VisitsSummary()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var total = await _db.SiteVisits.CountAsync();
        var todayCount = await _db.SiteVisits.CountAsync(v => v.CreatedAt >= today);
        var monthCount = await _db.SiteVisits.CountAsync(v => v.CreatedAt >= monthStart);

        return Ok(new
        {
            total,
            today = todayCount,
            month = monthCount,
            utc = now
        });
    }
}
