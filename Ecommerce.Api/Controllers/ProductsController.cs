using System;
using System.Linq;
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PublicProductQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 60 ? 12 : query.PageSize;
        var q = N(query.Q);
        var brand = N(query.Brand);
        var category = N(query.Category);
        var subCategory = N(query.SubCategory);

        var baseQuery = _db.Products
            .AsNoTracking()
            .Where(p => p.IsPublished);

        if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            baseQuery = baseQuery.Where(p => p.Brand != null && p.Brand.ToLower() == brand);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            baseQuery = baseQuery.Where(p => p.Category != null && p.Category.ToLower() == category);
        }

        if (!string.IsNullOrWhiteSpace(subCategory))
        {
            baseQuery = baseQuery.Where(p => p.SubCategory != null && p.SubCategory.ToLower() == subCategory);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            baseQuery = baseQuery.Where(p =>
                (p.Title != null && p.Title.ToLower().Contains(q)) ||
                (p.Description != null && p.Description.ToLower().Contains(q)) ||
                (p.Slug != null && p.Slug.ToLower().Contains(q)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(q)));
        }

        baseQuery = (query.Sort ?? "new") switch
        {
            "price:asc" or "priceAsc" => baseQuery.OrderBy(p => p.PriceIqd),
            "price:desc" or "priceDesc" => baseQuery.OrderByDescending(p => p.PriceIqd),
            _ => baseQuery.OrderByDescending(p => p.CreatedAt),
        };

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
        var safeTake = take is < 1 or > 60 ? 24 : take;
        var items = await BuildProjectedProductsAsync(
            _db.Products.AsNoTracking()
                .Where(p => p.IsPublished && p.DiscountPercent > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(safeTake)
        );

        return Ok(new { totalCount = items.Count, items });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 8)
    {
        var nq = N(q);
        if (string.IsNullOrWhiteSpace(nq)) return Ok(Array.Empty<object>());
        var safeLimit = limit is < 1 or > 20 ? 8 : limit;

        var query = _db.Products.AsNoTracking()
            .Where(p => p.IsPublished && (
                (p.Title != null && p.Title.ToLower().Contains(nq)) ||
                (p.Description != null && p.Description.ToLower().Contains(nq)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(nq)) ||
                (p.Slug != null && p.Slug.ToLower().Contains(nq)) ||
                (p.Category != null && p.Category.ToLower().Contains(nq)) ||
                (p.SubCategory != null && p.SubCategory.ToLower().Contains(nq))
            ))
            .OrderByDescending(p => p.CreatedAt)
            .Take(safeLimit)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.PriceIqd,
                p.DiscountPercent,
                finalPriceIqd = p.DiscountPercent > 0
                    ? Math.Round(p.PriceIqd * (100m - p.DiscountPercent) / 100m, 2)
                    : p.PriceIqd,
                p.Brand,
                p.Category,
                coverImage = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
            });

        return Ok(await query.ToListAsync());
    }

    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> TrackView([FromRoute] Guid id)
    {
        var exists = await _db.Products.AsNoTracking().AnyAsync(x => x.IsPublished && x.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });

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
