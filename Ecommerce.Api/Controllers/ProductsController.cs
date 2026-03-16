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

    private IQueryable<object> BuildProjectedProducts(IQueryable<Domain.Entities.Product> query)
    {
        return query.Select(p => new
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
        });
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

        IQueryable<Domain.Entities.Product> baseQuery = _db.Products
            .AsNoTracking()
            .Where(p => p.IsPublished);

        if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("all", StringComparison.OrdinalIgnoreCase))
            baseQuery = baseQuery.Where(p => p.Brand != null && p.Brand.ToLower() == brand);

        // التصنيف يجب أن يعتمد فقط على الحقل المخزن داخل المنتج
        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            baseQuery = baseQuery.Where(p => p.Category != null && p.Category.ToLower() == category);

        if (!string.IsNullOrWhiteSpace(subCategory) && !subCategory.Equals("all", StringComparison.OrdinalIgnoreCase))
            baseQuery = baseQuery.Where(p => p.SubCategory != null && p.SubCategory.ToLower() == subCategory);

        // البحث النصي يبقى مستقلاً ولا يغير منطق التصنيف
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            baseQuery = baseQuery.Where(p =>
                EF.Functions.ILike(p.Title ?? string.Empty, like) ||
                EF.Functions.ILike(p.Description ?? string.Empty, like) ||
                EF.Functions.ILike(p.Slug ?? string.Empty, like) ||
                EF.Functions.ILike(p.Brand ?? string.Empty, like));
        }

        var projected = BuildProjectedProducts(baseQuery);

        projected = (query.Sort ?? "new") switch
        {
            "price:asc" or "priceAsc" => projected.OrderBy(p => EF.Property<decimal>(p, "PriceIqd")),
            "price:desc" or "priceDesc" => projected.OrderByDescending(p => EF.Property<decimal>(p, "PriceIqd")),
            _ => projected.OrderByDescending(p => EF.Property<DateTime>(p, "CreatedAt")),
        };

        var total = await projected.CountAsync();
        var items = await projected.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { page, pageSize, totalCount = total, items });
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int take = 12)
    {
        var safeTake = take is < 1 or > 60 ? 12 : take;
        var items = await BuildProjectedProducts(_db.Products.AsNoTracking().Where(p => p.IsPublished && p.IsFeatured).OrderByDescending(p => p.CreatedAt).Take(safeTake)).ToListAsync();
        if (items.Count == 0)
            items = await BuildProjectedProducts(_db.Products.AsNoTracking().Where(p => p.IsPublished).OrderByDescending(p => p.CreatedAt).Take(safeTake)).ToListAsync();
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

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 8)
    {
        q = q?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());
        var safeLimit = limit is < 1 or > 20 ? 8 : limit;
        var like = $"%{q}%";
        var items = await _db.Products.AsNoTracking()
            .Where(p => p.IsPublished &&
                (EF.Functions.ILike(p.Title ?? string.Empty, like)
                || EF.Functions.ILike(p.Description ?? string.Empty, like)
                || EF.Functions.ILike(p.Brand ?? string.Empty, like)
                || EF.Functions.ILike(p.Slug ?? string.Empty, like)))
            .OrderByDescending(p => p.CreatedAt)
            .Take(safeLimit)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.PriceIqd,
                p.PriceUsd,
                p.Brand,
                coverImage = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
            }).ToListAsync();
        return Ok(items);
    }

    [HttpGet("discounts")]
    public async Task<IActionResult> GetDiscounts([FromQuery] int take = 24)
    {
        var safeTake = take is < 1 or > 100 ? 24 : take;
        var items = await BuildProjectedProducts(_db.Products.AsNoTracking()
            .Where(p => p.IsPublished && p.DiscountPercent > 0)
            .OrderByDescending(p => p.DiscountPercent)
            .ThenByDescending(p => p.CreatedAt)
            .Take(safeTake)).ToListAsync();

        return Ok(new { totalCount = items.Count, items });
    }
}
