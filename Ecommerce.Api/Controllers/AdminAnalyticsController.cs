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
        if (days < 1) days = 3650;
        var since = DateTime.UtcNow.AddDays(-days);

        async Task<List<(Guid productId, int count)>> GetPurchased(DateTime? from)
        {
            var q = _db.Orders.AsQueryable();
            if (from.HasValue) q = q.Where(o => o.CreatedAt >= from.Value);

            var purchasedRows = await q
                .SelectMany(o => o.Items.Where(i => i.ProductId != null)
                    .Select(i => new { productId = i.ProductId!.Value, qty = i.Quantity }))
                .ToListAsync();

            return purchasedRows
                .GroupBy(x => x.productId)
                .Select(g => (productId: g.Key, count: g.Sum(z => z.qty)))
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToList();
        }

        async Task<List<(Guid productId, int count)>> GetFavorited(DateTime? from)
        {
            var q = _db.Favorites.AsQueryable();
            if (from.HasValue) q = q.Where(f => f.CreatedAt >= from.Value);

            return (await q
                .GroupBy(f => f.ProductId)
                .Select(g => new { productId = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync())
                .Select(x => (x.productId, x.count))
                .ToList();
        }

        async Task<List<(Guid productId, int count)>> GetViewed(DateTime? from)
        {
            var q = _db.ProductViews.AsQueryable();
            if (from.HasValue) q = q.Where(v => v.CreatedAt >= from.Value);

            return (await q
                .GroupBy(v => v.ProductId)
                .Select(g => new { productId = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync())
                .Select(x => (x.productId, x.count))
                .ToList();
        }

        var topPurchasedAgg = await GetPurchased(since);
        var topFavoritedAgg = await GetFavorited(since);
        var topViewedAgg = await GetViewed(since);

        // fallback إلى كل التاريخ إذا المدة الحالية ما رجعت نتائج
        if (topPurchasedAgg.Count == 0) topPurchasedAgg = await GetPurchased(null);
        if (topFavoritedAgg.Count == 0) topFavoritedAgg = await GetFavorited(null);
        if (topViewedAgg.Count == 0) topViewedAgg = await GetViewed(null);

        // neglected: أقل المنتجات تفاعلاً (مشاهدة + مفضلة + شراء) مع fallback لكل التاريخ
        async Task<List<dynamic>> BuildNeglected(DateTime? from)
        {
            var productsBase = await _db.Products
                .Where(p => p.IsPublished)
                .Select(p => new { p.Id, p.Title, p.Slug, p.Brand, p.PriceIqd })
                .ToListAsync();

            var viewsQ = _db.ProductViews.AsQueryable();
            var favsQ = _db.Favorites.AsQueryable();
            var ordersQ = _db.Orders.AsQueryable();
            if (from.HasValue)
            {
                viewsQ = viewsQ.Where(v => v.CreatedAt >= from.Value);
                favsQ = favsQ.Where(f => f.CreatedAt >= from.Value);
                ordersQ = ordersQ.Where(o => o.CreatedAt >= from.Value);
            }

            var viewsMap = await viewsQ.GroupBy(v => v.ProductId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count);
            var favsMap = await favsQ.GroupBy(v => v.ProductId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count);
            var purchasesMap = (await ordersQ
                .SelectMany(o => o.Items.Where(i => i.ProductId != null).Select(i => new { productId = i.ProductId!.Value, qty = i.Quantity }))
                .ToListAsync())
                .GroupBy(x => x.productId)
                .ToDictionary(g => g.Key, g => g.Sum(z => z.qty));

            return productsBase
                .Select(p => new
                {
                    productId = p.Id,
                    title = p.Title,
                    views = viewsMap.TryGetValue(p.Id, out var vv) ? vv : 0,
                    favorites = favsMap.TryGetValue(p.Id, out var ff) ? ff : 0,
                    purchases = purchasesMap.TryGetValue(p.Id, out var pp) ? pp : 0,
                    score = (viewsMap.TryGetValue(p.Id, out vv) ? vv : 0) + (favsMap.TryGetValue(p.Id, out ff) ? ff : 0) + (purchasesMap.TryGetValue(p.Id, out pp) ? pp : 0)
                })
                .OrderBy(x => x.score)
                .ThenBy(x => x.views)
                .ThenBy(x => x.favorites)
                .ThenBy(x => x.purchases)
                .Take(10)
                .Cast<dynamic>()
                .ToList();
        }

        var neglected = await BuildNeglected(since);
        if (neglected.Count == 0) neglected = await BuildNeglected(null);

        var ids = topFavoritedAgg.Select(x => x.productId)
            .Concat(topViewedAgg.Select(x => x.productId))
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
            topFavorites = topFavoritedAgg.Select(x => new
            {
                productId = x.productId,
                title = products.FirstOrDefault(p => p.Id == x.productId)?.Title,
                favorites = x.count
            }),
            topViews = topViewedAgg.Select(x => new
            {
                productId = x.productId,
                title = products.FirstOrDefault(p => p.Id == x.productId)?.Title,
                views = x.count
            }),
            neglected
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
                users = usersSeries.TryGetValue(k, out var u) ? u : 0
            }).ToList();
        }

        var daily = await Build("daily", sinceDaily);
        var monthly = await Build("monthly", sinceMonthly);

        return Ok(new { daily, monthly });
    }
}
