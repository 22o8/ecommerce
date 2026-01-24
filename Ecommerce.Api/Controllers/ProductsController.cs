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
