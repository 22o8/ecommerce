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

        async Task<List<(Guid productId, int count)>> BuildFavorites(DateTime? window)
        {
            var query = _db.Favorites.AsQueryable();
            if (window.HasValue) query = query.Where(f => f.CreatedAt >= window.Value);
            var list = await query
                .GroupBy(f => f.ProductId)
                .Select(g => new { productId = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync();
            return list.Select(x => (x.productId, x.count)).ToList();
        }

        async Task<List<(Guid productId, int count)>> BuildViews(DateTime? window)
        {
            var query = _db.ProductViews.AsQueryable();
            if (window.HasValue) query = query.Where(v => v.CreatedAt >= window.Value);
            var list = await query
                .GroupBy(v => v.ProductId)
                .Select(g => new { productId = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync();
            return list.Select(x => (x.productId, x.count)).ToList();
        }

        async Task<List<(Guid productId, int count)>> BuildPurchases(DateTime? window)
        {
            var query = _db.Orders.AsQueryable();
            if (window.HasValue) query = query.Where(o => o.CreatedAt >= window.Value);
            var purchased = await query
                .SelectMany(o => o.Items.Where(i => i.ProductId != null)
                    .Select(i => new { productId = i.ProductId!.Value, qty = i.Quantity }))
                .ToListAsync();

            return purchased
                .GroupBy(x => x.productId)
                .Select(g => (productId: g.Key, count: g.Sum(z => z.qty)))
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToList();
        }

        var topFavorited = await BuildFavorites(since);
        if (topFavorited.Count == 0) topFavorited = await BuildFavorites(null);

        var topViewed = await BuildViews(since);
        if (topViewed.Count == 0) topViewed = await BuildViews(null);

        var topPurchasedAgg = await BuildPurchases(since);
        if (topPurchasedAgg.Count == 0) topPurchasedAgg = await BuildPurchases(null);

        // neglected: products with lowest engagement in selected range; if no range data, fallback to all-time
        var viewAgg = await _db.ProductViews
            .Where(v => v.CreatedAt >= since)
            .GroupBy(v => v.ProductId)
            .Select(g => new { productId = g.Key, views = g.Count() })
            .ToDictionaryAsync(x => x.productId, x => x.views);

        var favAgg = await _db.Favorites
            .Where(f => f.CreatedAt >= since)
            .GroupBy(f => f.ProductId)
            .Select(g => new { productId = g.Key, favorites = g.Count() })
            .ToDictionaryAsync(x => x.productId, x => x.favorites);

        var purchaseAgg = (await _db.Orders
            .Where(o => o.CreatedAt >= since)
            .SelectMany(o => o.Items.Where(i => i.ProductId != null)
                .Select(i => new { productId = i.ProductId!.Value, qty = i.Quantity }))
            .ToListAsync())
            .GroupBy(x => x.productId)
            .ToDictionary(g => g.Key, g => g.Sum(z => z.qty));

        var publishedProducts = await _db.Products
            .Where(p => p.IsPublished)
            .Select(p => new { p.Id, p.Title, p.Slug, p.Brand, p.PriceIqd, p.CreatedAt })
            .ToListAsync();

        var neglectedRows = publishedProducts
            .Select(p => new
            {
                productId = p.Id,
                title = p.Title,
                views = viewAgg.TryGetValue(p.Id, out var vv) ? vv : 0,
                favorites = favAgg.TryGetValue(p.Id, out var ff) ? ff : 0,
                purchases = purchaseAgg.TryGetValue(p.Id, out var pp) ? pp : 0,
                createdAt = p.CreatedAt
            })
            .OrderBy(x => x.views + x.favorites + x.purchases)
            .ThenByDescending(x => x.createdAt)
            .Take(10)
            .ToList();

        if (neglectedRows.All(x => (x.views + x.favorites + x.purchases) == 0))
        {
            // all rows zero in current window: fallback to all-time least engaged
            var allViewAgg = await _db.ProductViews
                .GroupBy(v => v.ProductId)
                .Select(g => new { productId = g.Key, views = g.Count() })
                .ToDictionaryAsync(x => x.productId, x => x.views);

            var allFavAgg = await _db.Favorites
                .GroupBy(f => f.ProductId)
                .Select(g => new { productId = g.Key, favorites = g.Count() })
                .ToDictionaryAsync(x => x.productId, x => x.favorites);

            var allPurchaseAgg = (await _db.Orders
                .SelectMany(o => o.Items.Where(i => i.ProductId != null)
                    .Select(i => new { productId = i.ProductId!.Value, qty = i.Quantity }))
                .ToListAsync())
                .GroupBy(x => x.productId)
                .ToDictionary(g => g.Key, g => g.Sum(z => z.qty));

            neglectedRows = publishedProducts
                .Select(p => new
                {
                    productId = p.Id,
                    title = p.Title,
                    views = allViewAgg.TryGetValue(p.Id, out var vv) ? vv : 0,
                    favorites = allFavAgg.TryGetValue(p.Id, out var ff) ? ff : 0,
                    purchases = allPurchaseAgg.TryGetValue(p.Id, out var pp) ? pp : 0,
                    createdAt = p.CreatedAt
                })
                .OrderBy(x => x.views + x.favorites + x.purchases)
                .ThenByDescending(x => x.createdAt)
                .Take(10)
                .ToList();
        }

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
            neglected = neglectedRows
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
            var users = await _db.Users.Where(u => u.CreatedAt >= since).Select(u => u.CreatedAt).ToListAsync();

            var ordersSeries = orders.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var viewsSeries = views.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var favsSeries = favs.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var visitsSeries = visits.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var usersSeries = users.Select(Bucket).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var keys = ordersSeries.Keys
                .Concat(viewsSeries.Keys)
                .Concat(favsSeries.Keys)
                .Concat(visitsSeries.Keys)
                .Concat(usersSeries.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return keys.Select(k => (dynamic)new
            {
                period = mode == "monthly" ? k.ToString("yyyy-MM") : k.ToString("yyyy-MM-dd"),
                orders = ordersSeries.TryGetValue(k, out var o) ? o : 0,
                views = viewsSeries.TryGetValue(k, out var v) ? v : 0,
                favorites = favsSeries.TryGetValue(k, out var f) ? f : 0,
                visits = visitsSeries.TryGetValue(k, out var s) ? s : 0,
                users = usersSeries.TryGetValue(k, out var u) ? u : 0,
                label = mode == "monthly" ? k.ToString("MM") : k.Day.ToString()
            }).ToList();
        }

        var daily = await Build("daily", sinceDaily);
        var monthly = await Build("monthly", sinceMonthly);

        return Ok(new { daily, monthly });
    }
}
