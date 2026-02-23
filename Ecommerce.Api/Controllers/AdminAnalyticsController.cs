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

        return Ok(new
        {
            since,
            topFavorited = topFavorited.Select(x => new { x.productId, x.count, product = products.FirstOrDefault(p => p.Id == x.productId) }),
            topViewed = topViewed.Select(x => new { x.productId, x.count, product = products.FirstOrDefault(p => p.Id == x.productId) }),
            topPurchased = topPurchasedAgg.Select(x => new { x.productId, x.count, product = products.FirstOrDefault(p => p.Id == x.productId) }),
            neglected
        });
    }

    // GET /api/admin/analytics/activity?mode=daily&days=30
    [HttpGet("activity")]
    public async Task<IActionResult> Activity([FromQuery] string mode = "daily", [FromQuery] int days = 30)
    {
        if (days < 1) days = 30;
        var since = DateTime.UtcNow.AddDays(-days);

        // get raw events
        var orders = await _db.Orders.Where(o => o.CreatedAt >= since)
            .Select(o => o.CreatedAt)
            .ToListAsync();

        var users = await _db.Users.Where(u => u.CreatedAt >= since)
            .Select(u => u.CreatedAt)
            .ToListAsync();

        var views = await _db.ProductViews.Where(v => v.CreatedAt >= since)
            .Select(v => v.CreatedAt)
            .ToListAsync();

        var favs = await _db.Favorites.Where(f => f.CreatedAt >= since)
            .Select(f => f.CreatedAt)
            .ToListAsync();

        DateTime Bucket(DateTime dt)
        {
            var d = dt.Date;
            if (mode.Equals("monthly", StringComparison.OrdinalIgnoreCase))
                return new DateTime(d.Year, d.Month, 1);
            return d;
        }

        var series = orders.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var usersSeries = users.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var viewsSeries = views.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var favsSeries = favs.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        // union all buckets
        var keys = series.Keys
            .Concat(usersSeries.Keys)
            .Concat(viewsSeries.Keys)
            .Concat(favsSeries.Keys)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var items = keys.Select(k => new
        {
            date = k,
            orders = series.TryGetValue(k, out var o) ? o : 0,
            newUsers = usersSeries.TryGetValue(k, out var u) ? u : 0,
            views = viewsSeries.TryGetValue(k, out var v) ? v : 0,
            favorites = favsSeries.TryGetValue(k, out var f) ? f : 0
        }).ToList();

        return Ok(new { since, mode, items });
    }
}
