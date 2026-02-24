using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminAnalyticsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/analytics/overview?days=30
    [HttpGet("overview")]
    public async Task<IActionResult> Overview([FromQuery] int days = 30)
    {
        if (days < 1) days = 30;
        var since = DateTime.UtcNow.AddDays(-days);

        var topFavorited = await _db.Favorites
            .Where(f => f.CreatedAt >= since)
            .GroupBy(f => f.ProductId)
            .Select(g => new { productId = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(10)
            .ToListAsync();

        var topViewed = await _db.ProductViews
            .Where(v => v.CreatedAt >= since)
            .GroupBy(v => v.ProductId)
            .Select(g => new { productId = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(10)
            .ToListAsync();

        // safer: query orders then flatten items
        var purchased = await _db.Orders
            .Where(o => o.CreatedAt >= since)
            .SelectMany(o => o.Items.Where(i => i.ProductId != null)
                .Select(i => new { productId = i.ProductId!.Value, qty = i.Quantity }))
            .ToListAsync();

        var topPurchasedAgg = purchased
            .GroupBy(x => x.productId)
            .Select(g => new { productId = g.Key, count = g.Sum(z => z.qty) })
            .OrderByDescending(x => x.count)
            .Take(10)
            .ToList();

        // neglected: published products with zero views in range
        var viewedIds = await _db.ProductViews
            .Where(v => v.CreatedAt >= since)
            .Select(v => v.ProductId)
            .Distinct()
            .ToListAsync();

        var neglected = await _db.Products
            .Where(p => p.IsPublished)
            .Where(p => !viewedIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(p => new { p.Id, p.Title, p.Slug, p.Brand, p.PriceIqd })
            .ToListAsync();

        // attach product titles for top lists
        var ids = topFavorited.Select(x => x.productId)
            .Concat(topViewed.Select(x => x.productId))
            .Concat(topPurchasedAgg.Select(x => x.productId))
            .Distinct()
            .ToList();

        var products = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.Title, p.Slug, p.Brand, p.PriceIqd })
            .ToListAsync();

        // ✅ Response shape matches frontend: /app/pages/admin/insights.vue
        return Ok(new
        {
            since,
            topPurchased = topPurchasedAgg.Select(x => new
            {
                productId = x.productId,
                title = products.FirstOrDefault(p => p.Id == x.productId)?.Title,
                purchases = x.count
            }),
            topFavorites = topFavorited.Select(x => new
            {
                productId = x.productId,
                title = products.FirstOrDefault(p => p.Id == x.productId)?.Title,
                favorites = x.count
            }),
            topViews = topViewed.Select(x => new
            {
                productId = x.productId,
                title = products.FirstOrDefault(p => p.Id == x.productId)?.Title,
                views = x.count
            }),
            neglected = neglected.Select(p => new
            {
                productId = p.Id,
                title = p.Title,
                views = 0,
                favorites = 0,
                purchases = 0
            })
        });
    }

    // GET /api/admin/analytics/activity
    // يرجّع day + month بنفس الوقت حتى يطابق الفرونت.
    [HttpGet("activity")]
    public async Task<IActionResult> Activity()
    {
        var now = DateTime.UtcNow;
        var sinceDaily = now.AddDays(-30);
        var sinceMonthly = now.AddMonths(-12);

        async Task<List<dynamic>> Build(string mode, DateTime since)
        {
            DateTime Bucket(DateTime dt)
            {
                var d = DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
                if (mode == "monthly") return new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return d;
            }

            var orders = await _db.Orders.Where(o => o.CreatedAt >= since).Select(o => o.CreatedAt).ToListAsync();
            var views = await _db.ProductViews.Where(v => v.CreatedAt >= since).Select(v => v.CreatedAt).ToListAsync();
            var favs = await _db.Favorites.Where(f => f.CreatedAt >= since).Select(f => f.CreatedAt).ToListAsync();
            var visits = await _db.SiteVisits.Where(v => v.CreatedAt >= since).Select(v => v.CreatedAt).ToListAsync();

            var ordersSeries = orders.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var viewsSeries = views.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var favsSeries = favs.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var visitsSeries = visits.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var keys = ordersSeries.Keys
                .Concat(viewsSeries.Keys)
                .Concat(favsSeries.Keys)
                .Concat(visitsSeries.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return keys.Select(k => (dynamic)new
            {
                period = mode == "monthly" ? k.ToString("yyyy-MM") : k.ToString("yyyy-MM-dd"),
                orders = ordersSeries.TryGetValue(k, out var o) ? o : 0,
                views = viewsSeries.TryGetValue(k, out var v) ? v : 0,
                favorites = favsSeries.TryGetValue(k, out var f) ? f : 0,
                visits = visitsSeries.TryGetValue(k, out var s) ? s : 0
            }).ToList();
        }

        var daily = await Build("daily", sinceDaily);
        var monthly = await Build("monthly", sinceMonthly);

        return Ok(new { daily, monthly });
    }
}
