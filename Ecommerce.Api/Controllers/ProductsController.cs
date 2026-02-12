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

    // GET /api/Products?Page=1&PageSize=12&Q=abc&Sort=new|price:asc|price:desc
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PublicProductQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 60 ? 12 : query.PageSize;
        var q = (query.Q ?? "").Trim().ToLowerInvariant();

        var baseQuery = _db.Products
            .AsNoTracking()
            .Where(p => p.IsPublished);

        if (!string.IsNullOrWhiteSpace(q))
        {
            baseQuery = baseQuery.Where(p =>
                p.Title.ToLower().Contains(q) ||
                (p.Description != null && p.Description.ToLower().Contains(q)) ||
                p.Slug.ToLower().Contains(q)
            );
        }

        var brand = (query.Brand ?? "").Trim();

        if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            baseQuery = baseQuery.Where(p => p.Brand != null && p.Brand.ToLower() == brand.ToLower());
        }

        baseQuery = (query.Sort ?? "new") switch
        {
            "price:asc" or "priceAsc" => baseQuery.OrderBy(p => p.PriceUsd),
            "price:desc" or "priceDesc" => baseQuery.OrderByDescending(p => p.PriceUsd),
            _ => baseQuery.OrderByDescending(p => p.CreatedAt),
        };

        var total = await baseQuery.CountAsync();

        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.Description,
                p.PriceUsd,
                p.RatingAvg,
                p.Brand,
                p.RatingCount,
                p.CreatedAt,
                coverImage = _db.ProductImages
                    .Where(i => i.ProductId == p.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalCount = total,
            items
        });
    }

    // GET /api/Products/featured
	// ✅ Home page "Featured Products"
	// المطلوب: منتجات يحددها الأدمن (IsFeatured = true). وإذا ماكو أي منتج مميز، نرجّع أحدث منتجات منشورة كـ fallback.
    [HttpGet("featured")]
	public async Task<IActionResult> GetFeatured([FromQuery] int take = 12)
    {
        var safeTake = take is < 1 or > 60 ? 12 : take;

        // 1) Featured first
        var featured = await _db.Products
            .AsNoTracking()
			.Where(p => p.IsPublished && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(safeTake)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.Description,
                p.PriceUsd,
                p.RatingAvg,
                p.Brand,
                p.RatingCount,
                p.CreatedAt,
                coverImage = _db.ProductImages
                    .Where(i => i.ProductId == p.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault()
            })
            .ToListAsync();

        // 2) Fallback: latest published
        var items = featured;
        if (items.Count == 0)
        {
            items = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .Take(safeTake)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Description,
                    p.PriceUsd,
                    p.RatingAvg,
                    p.Brand,
                    p.RatingCount,
                    p.CreatedAt,
                    coverImage = _db.ProductImages
                        .Where(i => i.ProductId == p.Id)
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.Url)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        return Ok(new
        {
            totalCount = items.Count,
            items
        });
    }

    // GET /api/Products/{id}
    // Public product details by id (compatibility for some frontend pages)
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.IsPublished && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceUsd,
                x.RatingAvg,
                x.Brand,
                x.RatingCount,
                x.CreatedAt,
                images = _db.ProductImages
                    .Where(i => i.ProductId == x.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (p == null) return NotFound(new { message = "Product not found" });
        return Ok(p);
    }

    // GET /api/Products/{id}/images
    // Public images list by product id (compatibility)
    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages([FromRoute] Guid id)
    {
        var exists = await _db.Products.AsNoTracking().AnyAsync(x => x.IsPublished && x.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });

        var images = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == id)
            .OrderBy(i => i.SortOrder)
            .Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder })
            .ToListAsync();

        return Ok(images);
    }

    // GET /api/Products/slug/{slug}
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
        slug = (slug ?? "").Trim().ToLowerInvariant();

        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.IsPublished && x.Slug.ToLower() == slug)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceUsd,
                x.RatingAvg,
                x.Brand,
                x.RatingCount,
                x.CreatedAt,
                images = _db.ProductImages
                    .Where(i => i.ProductId == x.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (p == null) return NotFound(new { message = "Product not found" });
        return Ok(p);
    }
}
