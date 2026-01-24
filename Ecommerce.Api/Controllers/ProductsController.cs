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
                p.RatingCount,
                p.CreatedAt,
                coverImageUrl = _db.ProductImages
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
                x.RatingCount,
                x.CreatedAt,
                coverImageUrl = _db.ProductImages
                    .Where(i => i.ProductId == x.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
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

    // Backward-compatible endpoint used by the frontend BFF: /api/Products/by-slug?slug=...
    [HttpGet("by-slug")]
    public async Task<IActionResult> GetBySlugQuery([FromQuery] string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return BadRequest(new { message = "slug is required" });
        return await GetBySlug(slug);
    }

    // Public: get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceUsd,
                x.RatingAvg,
                x.RatingCount,
                x.CreatedAt,
                coverImageUrl = _db.ProductImages
                    .Where(i => i.ProductId == x.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
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

    // Public: list images for a product
    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(Guid id)
    {
        var exists = await _db.Products.AsNoTracking().AnyAsync(p => p.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });

        var images = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == id)
            .OrderBy(i => i.SortOrder)
            .Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder })
            .ToListAsync();

        return Ok(new { items = images });
    }
}
