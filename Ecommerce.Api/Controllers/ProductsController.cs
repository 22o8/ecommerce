using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Models.Products;
using Microsoft.AspNetCore.Authorization;
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

    private Guid? CurrentUserId
    {
        get
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User?.FindFirst("sub")?.Value
                        ?? User?.FindFirst("userId")?.Value;
            return Guid.TryParse(claim, out var parsed) ? parsed : null;
        }
    }

    private static readonly Dictionary<string, string[]> CategoryKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["moisturizer"] = new[] { "مرطب", "مرطب الوجه", "كريم مرطب", "جل مرطب", "moisturizer", "moisturiser", "hydrating cream", "hydrating gel", "lotion", "cream" },
        ["eye-care"] = new[] { "eye", "عين", "under eye", "undereye", "eye cream", "eye serum", "eye gel" },
        ["cleanser"] = new[] { "cleanser", "cleanse", "غسول", "foam wash", "face wash", "منظف" },
        ["serum"] = new[] { "serum", "سيروم", "ampoule" },
        ["sunscreen"] = new[] { "sunscreen", "sun screen", "spf", "واقي", "واقي شمس" },
        ["toner"] = new[] { "toner", "تونر" },
        ["mask"] = new[] { "mask", "ماسك" },
    };

    private static readonly Dictionary<string, string[]> SubCategoryKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eye-serum"] = new[] { "eye serum", "serum eye", "سيروم العين", "سيروم للعين" },
        ["eye-cream"] = new[] { "eye cream", "cream eye", "كريم العين", "كريم للعين" },
        ["eye-gel"] = new[] { "eye gel", "جل العين", "جل للعين" },
        ["face-cream"] = new[] { "face cream", "cream", "كريم", "moisturizing cream" },
        ["face-gel"] = new[] { "gel", "جل", "moisturizing gel" },
        ["foam-cleanser"] = new[] { "foam", "رغوي", "foam cleanser" },
        ["oil-cleanser"] = new[] { "oil cleanser", "cleansing oil", "زيتي" },
    };

    public record SaveRatingRequest(int Rating, string? Comment);

    private static bool ContainsAny(string haystack, IEnumerable<string> needles)
        => needles.Any(n => haystack.Contains(n, StringComparison.OrdinalIgnoreCase));

    private static bool MatchesCategory(dynamic p, string? category, string? subCategory, string? q)
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

        var normalizedCategory = N(category);
        var normalizedSub = N(subCategory);
        var normalizedQ = N(q);

        if (!string.IsNullOrWhiteSpace(normalizedCategory))
        {
            var storedCat = N((string?)p.Category);
            if (storedCat == normalizedCategory)
            {
            }
            else if (CategoryKeywords.TryGetValue(normalizedCategory, out var catWords) && ContainsAny(text, catWords))
            {
            }
            else
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(normalizedSub))
        {
            var storedSub = N((string?)p.SubCategory);
            if (storedSub == normalizedSub)
            {
            }
            else if (SubCategoryKeywords.TryGetValue(normalizedSub, out var subWords) && ContainsAny(text, subWords))
            {
            }
            else
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(normalizedQ))
        {
            if (!text.Contains(normalizedQ))
            {
                if (SubCategoryKeywords.TryGetValue(normalizedQ, out var exactSubWords) && ContainsAny(text, exactSubWords))
                {
                }
                else if (CategoryKeywords.TryGetValue(normalizedQ, out var exactCatWords) && ContainsAny(text, exactCatWords))
                {
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    private async Task<List<dynamic>> BuildProjectedProductsAsync(IQueryable<Product> query)
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

    private async Task RefreshProductRatingAsync(Guid productId)
    {
        var stats = await _db.ProductReviews
            .Where(x => x.ProductId == productId)
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                Count = g.Count(),
                Avg = g.Average(x => (decimal)x.Rating)
            })
            .FirstOrDefaultAsync();

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null) return;

        product.RatingCount = stats?.Count ?? 0;
        product.RatingAvg = stats == null ? 0m : Math.Round(stats.Avg, 2);
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

        var baseQuery = _db.Products.AsNoTracking().Where(p => p.IsPublished);

        if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("all", StringComparison.OrdinalIgnoreCase))
            baseQuery = baseQuery.Where(p => p.Brand != null && p.Brand.ToLower() == brand);

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            baseQuery = baseQuery.Where(p =>
                (p.Category != null && p.Category.ToLower() == category) ||
                (p.SubCategory != null && p.SubCategory.ToLower() == category));

        if (!string.IsNullOrWhiteSpace(subCategory) && !subCategory.Equals("all", StringComparison.OrdinalIgnoreCase))
            baseQuery = baseQuery.Where(p => p.SubCategory != null && p.SubCategory.ToLower() == subCategory);

        if (!string.IsNullOrWhiteSpace(q))
        {
            baseQuery = baseQuery.Where(p =>
                (p.Title != null && p.Title.ToLower().Contains(q)) ||
                (p.Description != null && p.Description.ToLower().Contains(q)) ||
                (p.Slug != null && p.Slug.ToLower().Contains(q)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(q)));
        }

        if ((query.Sort ?? "new").Equals("rating", StringComparison.OrdinalIgnoreCase) ||
            (query.Sort ?? "new").Equals("topRated", StringComparison.OrdinalIgnoreCase))
        {
            baseQuery = baseQuery.Where(p => p.RatingCount > 0)
                .OrderByDescending(p => p.RatingAvg)
                .ThenByDescending(p => p.RatingCount)
                .ThenByDescending(p => p.CreatedAt);
        }
        else
        {
            baseQuery = (query.Sort ?? "new") switch
            {
                "price:asc" or "priceAsc" => baseQuery.OrderBy(p => p.PriceIqd),
                "price:desc" or "priceDesc" => baseQuery.OrderByDescending(p => p.PriceIqd),
                _ => baseQuery.OrderByDescending(p => p.CreatedAt),
            };
        }

        var total = await baseQuery.CountAsync();
        var pagedQuery = baseQuery.Skip((page - 1) * pageSize).Take(pageSize);
        var items = await BuildProjectedProductsAsync(pagedQuery);

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

    [HttpGet("top-rated")]
    public async Task<IActionResult> GetTopRated([FromQuery] int take = 8)
    {
        var safeTake = take is < 1 or > 60 ? 8 : take;
        var items = await BuildProjectedProductsAsync(
            _db.Products.AsNoTracking()
                .Where(p => p.IsPublished && p.RatingCount > 0)
                .OrderByDescending(p => p.RatingAvg)
                .ThenByDescending(p => p.RatingCount)
                .ThenByDescending(p => p.CreatedAt)
                .Take(safeTake)
        );

        return Ok(new { totalCount = items.Count, items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var currentUserId = CurrentUserId;
        var p = await _db.Products.AsNoTracking().Where(x => x.IsPublished && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceIqd,
                x.DiscountPercent,
                finalPriceIqd = x.DiscountPercent > 0 ? Math.Round(x.PriceIqd * (100m - x.DiscountPercent) / 100m, 2) : x.PriceIqd,
                x.PriceUsd,
                x.RatingAvg,
                x.Brand,
                x.Category,
                x.SubCategory,
                x.StockQuantity,
                x.IsCouponAllowed,
                x.RatingCount,
                x.CreatedAt,
                viewCount = _db.ProductViews.Count(v => v.ProductId == x.Id),
                favoriteCount = _db.Favorites.Count(f => f.ProductId == x.Id),
                images = _db.ProductImages.Where(i => i.ProductId == x.Id).OrderBy(i => i.SortOrder).Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder }).ToList(),
                reviews = _db.ProductReviews.Where(r => r.ProductId == x.Id).OrderByDescending(r => r.UpdatedAt).Take(10).Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    r.UpdatedAt,
                    userId = r.UserId,
                    userName = _db.Users.Where(u => u.Id == r.UserId).Select(u => u.FullName).FirstOrDefault()
                }).ToList(),
                myReview = currentUserId == null ? null : _db.ProductReviews.Where(r => r.ProductId == x.Id && r.UserId == currentUserId).Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    r.UpdatedAt
                }).FirstOrDefault()
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
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceIqd,
                x.DiscountPercent,
                finalPriceIqd = x.DiscountPercent > 0 ? Math.Round(x.PriceIqd * (100m - x.DiscountPercent) / 100m, 2) : x.PriceIqd,
                x.PriceUsd,
                x.RatingAvg,
                x.Brand,
                x.Category,
                x.SubCategory,
                x.StockQuantity,
                x.IsCouponAllowed,
                x.RatingCount,
                x.CreatedAt,
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
        return Ok(items.Where(p => MatchesCategory(p, null, null, qq)).Take(safeLimit).ToList());
    }

    [HttpPost("{id:guid}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackView(Guid id)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();

        _db.ProductViews.Add(new ProductView { ProductId = id, UserId = CurrentUserId });
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [HttpPost("{id:guid}/rate")]
    [Authorize]
    public async Task<IActionResult> SaveRating(Guid id, [FromBody] SaveRatingRequest req)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized(new { message = "يجب تسجيل الدخول أولاً" });

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && x.IsPublished);
        if (product == null) return NotFound(new { message = "المنتج غير موجود" });

        var rating = Math.Clamp(req.Rating, 1, 5);
        var comment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim();
        if (comment?.Length > 1500) comment = comment[..1500];

        var existing = await _db.ProductReviews.FirstOrDefaultAsync(x => x.ProductId == id && x.UserId == userId);
        if (existing == null)
        {
            _db.ProductReviews.Add(new ProductReview
            {
                ProductId = id,
                UserId = userId.Value,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Rating = rating;
            existing.Comment = comment;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await RefreshProductRatingAsync(id);
        await _db.SaveChangesAsync();

        return Ok(new { message = existing == null ? "تم إضافة التقييم بنجاح" : "تم تحديث التقييم بنجاح" });
    }
}
