using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    private static string N(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static string Slugify(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var text = value.Trim().ToLowerInvariant();
        var sb = new StringBuilder(text.Length);
        var prevDash = false;
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                prevDash = false;
            }
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_' or '/' or ':' or '.')
            {
                if (!prevDash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevDash = true;
                }
            }
        }
        return sb.ToString().Trim('-');
    }

    private async Task<HashSet<string>> BuildBrandCandidatesAsync(string? brand)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var raw = (brand ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(raw)) return candidates;

        var normalized = N(raw);
        var slugifiedInput = Slugify(raw);
        candidates.Add(normalized);
        if (!string.IsNullOrWhiteSpace(slugifiedInput)) candidates.Add(slugifiedInput);

        var entity = await _db.Brands
            .AsNoTracking()
            .Where(b => b.IsActive && (b.Slug.ToLower() == normalized || b.Name.ToLower() == normalized || b.Slug.ToLower() == slugifiedInput || b.Name.ToLower() == slugifiedInput))
            .Select(b => new { b.Slug, b.Name })
            .FirstOrDefaultAsync();

        if (entity != null)
        {
            candidates.Add(N(entity.Slug));
            candidates.Add(N(entity.Name));
            var nameSlug = Slugify(entity.Name);
            if (!string.IsNullOrWhiteSpace(nameSlug)) candidates.Add(nameSlug);
        }

        return candidates;
    }

    private async Task<List<dynamic>> BuildProjectedProductsAsync(IQueryable<Domain.Entities.Product> query)
    {
        return await query
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.Description,
                p.PriceIqd,
                p.DiscountPercent,
                finalPriceIqd = p.DiscountPercent > 0
                    ? Math.Round(p.PriceIqd * (100m - p.DiscountPercent) / 100m, 2)
                    : p.PriceIqd,
                p.PriceUsd,
                p.RatingAvg,
                p.Brand,
                p.Category,
                p.SubCategory,
                p.StockQuantity,
                p.IsCouponAllowed,
                p.RatingCount,
                p.CreatedAt,
                viewCount = _db.ProductViews.Count(v => v.ProductId == p.Id),
                favoriteCount = _db.Favorites.Count(f => f.ProductId == p.Id),
                coverImage = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
            })
            .Cast<dynamic>()
            .ToListAsync();
    }

    private static IEnumerable<dynamic> ApplyExactFilters(IEnumerable<dynamic> source, string? brand, HashSet<string> brandCandidates, string? category, string? subCategory, string? q)
    {
        var filtered = source;

        if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(p =>
            {
                var stored = ((string?)p.Brand ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(stored)) return false;
                var storedLower = stored.ToLowerInvariant();
                var storedSlug = Slugify(stored);
                return brandCandidates.Contains(storedLower) || (!string.IsNullOrWhiteSpace(storedSlug) && brandCandidates.Contains(storedSlug));
            });
        }

        var normalizedCategory = N(category);
        if (!string.IsNullOrWhiteSpace(normalizedCategory))
            filtered = filtered.Where(p => N((string?)p.Category) == normalizedCategory);

        var normalizedSub = N(subCategory);
        if (!string.IsNullOrWhiteSpace(normalizedSub))
            filtered = filtered.Where(p => N((string?)p.SubCategory) == normalizedSub);

        var normalizedQ = N(q);
        if (!string.IsNullOrWhiteSpace(normalizedQ))
        {
            filtered = filtered.Where(p =>
            {
                var text = string.Join(" ", new[]
                {
                    (string?)p.Title,
                    (string?)p.Description,
                    (string?)p.Slug,
                    (string?)p.Brand,
                    (string?)p.Category,
                    (string?)p.SubCategory,
                }.Where(x => !string.IsNullOrWhiteSpace(x))).ToLowerInvariant();
                return text.Contains(normalizedQ);
            });
        }

        return filtered;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PublicProductQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 60 ? 12 : query.PageSize;
        var q = N(query.Q);
        var brand = N(query.Brand);
        var category = N(query.Category);
        var subCategory = N(query.SubCategory);
        var brandCandidates = await BuildBrandCandidatesAsync(brand);

        var baseQuery = _db.Products.AsNoTracking().Where(p => p.IsPublished);
        var allItems = await BuildProjectedProductsAsync(baseQuery);
        var filtered = ApplyExactFilters(allItems, brand, brandCandidates, category, subCategory, q);

        filtered = (query.Sort ?? "new") switch
        {
            "price:asc" or "priceAsc" => filtered.OrderBy(p => (decimal)p.PriceIqd),
            "price:desc" or "priceDesc" => filtered.OrderByDescending(p => (decimal)p.PriceIqd),
            _ => filtered.OrderByDescending(p => (DateTime)p.CreatedAt),
        };

        var total = filtered.Count();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new { page, pageSize, totalCount = total, items });
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int take = 12)
    {
        var safeTake = take is < 1 or > 60 ? 12 : take;
        var items = await BuildProjectedProductsAsync(_db.Products.AsNoTracking().Where(p => p.IsPublished && p.IsFeatured).OrderByDescending(p => p.CreatedAt).Take(safeTake));
        if (items.Count == 0)
            items = await BuildProjectedProductsAsync(_db.Products.AsNoTracking().Where(p => p.IsPublished).OrderByDescending(p => p.CreatedAt).Take(safeTake));
        return Ok(new { totalCount = items.Count, items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var p = await _db.Products.AsNoTracking().Where(x => x.IsPublished && x.Id == id)
            .Select(x => new
            {
                x.Id, x.Title, x.Slug, x.Description, x.PriceIqd, x.DiscountPercent,
                finalPriceIqd = x.DiscountPercent > 0 ? Math.Round(x.PriceIqd * (100m - x.DiscountPercent) / 100m, 2) : x.PriceIqd,
                x.PriceUsd, x.RatingAvg, x.Brand, x.Category, x.SubCategory, x.StockQuantity, x.IsCouponAllowed, x.RatingCount, x.CreatedAt,
                viewCount = _db.ProductViews.Count(v => v.ProductId == x.Id),
                favoriteCount = _db.Favorites.Count(f => f.ProductId == x.Id),
                images = _db.ProductImages.Where(i => i.ProductId == x.Id).OrderBy(i => i.SortOrder).Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder }).ToList()
            }).FirstOrDefaultAsync();
        if (p == null) return NotFound(new { message = "Product not found" });
        return Ok(p);
    }

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages([FromRoute] Guid id)
    {
        var exists = await _db.Products.AsNoTracking().AnyAsync(x => x.IsPublished && x.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });
        var images = await _db.ProductImages.AsNoTracking().Where(i => i.ProductId == id).OrderBy(i => i.SortOrder).Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder }).ToListAsync();
        return Ok(images);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
        slug = N(slug);
        var p = await _db.Products.AsNoTracking().Where(x => x.IsPublished && x.Slug.ToLower() == slug)
            .Select(x => new
            {
                x.Id, x.Title, x.Slug, x.Description, x.PriceIqd, x.DiscountPercent,
                finalPriceIqd = x.DiscountPercent > 0 ? Math.Round(x.PriceIqd * (100m - x.DiscountPercent) / 100m, 2) : x.PriceIqd,
                x.PriceUsd, x.RatingAvg, x.Brand, x.Category, x.SubCategory, x.StockQuantity, x.IsCouponAllowed, x.RatingCount, x.CreatedAt,
                viewCount = _db.ProductViews.Count(v => v.ProductId == x.Id),
                favoriteCount = _db.Favorites.Count(f => f.ProductId == x.Id),
                images = _db.ProductImages.Where(i => i.ProductId == x.Id).OrderBy(i => i.SortOrder).Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder }).ToList()
            }).FirstOrDefaultAsync();
        if (p == null) return NotFound(new { message = "Product not found" });
        return Ok(p);
    }

    [HttpGet("discounts")]
    public async Task<IActionResult> GetDiscounts([FromQuery] int take = 24)
    {
        var safeTake = take is < 1 or > 120 ? 24 : take;
        var items = await BuildProjectedProductsAsync(_db.Products.AsNoTracking().Where(p => p.IsPublished && p.DiscountPercent > 0).OrderByDescending(p => p.DiscountPercent).ThenByDescending(p => p.CreatedAt).Take(safeTake));
        return Ok(new { totalCount = items.Count, items });
    }

    [HttpGet("search")]
    public async Task<IActionResult> LiveSearch([FromQuery] string? q = null, [FromQuery] int limit = 8)
    {
        var qq = N(q);
        if (string.IsNullOrWhiteSpace(qq)) return Ok(Array.Empty<object>());
        var safeLimit = limit is < 1 or > 20 ? 8 : limit;
        var items = await BuildProjectedProductsAsync(_db.Products.AsNoTracking().Where(p => p.IsPublished).OrderByDescending(p => p.CreatedAt));
        return Ok(ApplyExactFilters(items, null, new HashSet<string>(), null, null, qq).Take(safeLimit).ToList());
    }

    [HttpPost("{id:guid}/view")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> TrackView(Guid id)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();

        Guid? userId = null;
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User?.FindFirst("sub")?.Value
                    ?? User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrWhiteSpace(claim) && Guid.TryParse(claim, out var parsed))
            userId = parsed;

        _db.ProductViews.Add(new Domain.Entities.ProductView { ProductId = id, UserId = userId });
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}
