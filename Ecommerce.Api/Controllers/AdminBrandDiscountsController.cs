using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/brand-discounts")]
[Authorize(Roles = "Admin")]
public class AdminBrandDiscountsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminBrandDiscountsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var brands = await _db.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                b.Id,
                b.Slug,
                b.Name,
                b.LogoUrl,
                b.IsActive,
                ProductCount = _db.Products.Count(p => p.Brand.ToLower() == b.Slug.ToLower()),
                PublishedProductCount = _db.Products.Count(p => p.Brand.ToLower() == b.Slug.ToLower() && p.IsPublished),
                DiscountedProductCount = _db.Products.Count(p => p.Brand.ToLower() == b.Slug.ToLower() && p.DiscountPercent > 0),
                MinDiscountPercent = _db.Products
                    .Where(p => p.Brand.ToLower() == b.Slug.ToLower() && p.DiscountPercent > 0)
                    .Select(p => (int?)p.DiscountPercent)
                    .Min(),
                MaxDiscountPercent = _db.Products
                    .Where(p => p.Brand.ToLower() == b.Slug.ToLower() && p.DiscountPercent > 0)
                    .Select(p => (int?)p.DiscountPercent)
                    .Max(),
                AverageDiscountPercent = _db.Products
                    .Where(p => p.Brand.ToLower() == b.Slug.ToLower() && p.DiscountPercent > 0)
                    .Select(p => (double?)p.DiscountPercent)
                    .Average(),
            })
            .ToListAsync();

        var items = brands.Select(b => new
        {
            b.Id,
            b.Slug,
            b.Name,
            b.LogoUrl,
            b.IsActive,
            b.ProductCount,
            b.PublishedProductCount,
            b.DiscountedProductCount,
            MinDiscountPercent = b.MinDiscountPercent ?? 0,
            MaxDiscountPercent = b.MaxDiscountPercent ?? 0,
            AverageDiscountPercent = b.AverageDiscountPercent == null ? 0 : Math.Round(b.AverageDiscountPercent.Value, 1),
            HasDiscount = b.DiscountedProductCount > 0,
            IsUniformDiscount = b.DiscountedProductCount > 0 && b.MinDiscountPercent == b.MaxDiscountPercent,
            CurrentDiscountPercent = b.DiscountedProductCount > 0 && b.MinDiscountPercent == b.MaxDiscountPercent
                ? b.MaxDiscountPercent ?? 0
                : 0,
            DiscountLabel = b.DiscountedProductCount == 0
                ? "بدون تخفيض"
                : (b.MinDiscountPercent == b.MaxDiscountPercent
                    ? $"{b.MaxDiscountPercent}%"
                    : $"مختلف {b.MinDiscountPercent}% - {b.MaxDiscountPercent}%")
        }).ToList();

        return Ok(new
        {
            totalBrands = items.Count,
            activeBrandDiscounts = items.Count(x => x.HasDiscount),
            totalProducts = items.Sum(x => x.ProductCount),
            discountedProducts = items.Sum(x => x.DiscountedProductCount),
            items
        });
    }

    [HttpGet("{brandSlug}/products")]
    public async Task<IActionResult> GetBrandProducts([FromRoute] string brandSlug)
    {
        var slug = NormalizeSlug(brandSlug);
        if (string.IsNullOrWhiteSpace(slug)) return BadRequest(new { message = "Brand is required" });

        var brandExists = await _db.Brands.AsNoTracking().AnyAsync(b => b.Slug.ToLower() == slug);
        if (!brandExists) return NotFound(new { message = "Brand not found" });

        var items = await _db.Products
            .AsNoTracking()
            .Where(p => p.Brand.ToLower() == slug)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.PriceIqd,
                p.DiscountPercent,
                FinalPriceIqd = p.DiscountPercent > 0
                    ? Math.Round(p.PriceIqd * (100m - p.DiscountPercent) / 100m, 2)
                    : p.PriceIqd,
                p.IsPublished,
                CoverImage = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
            })
            .Take(30)
            .ToListAsync();

        return Ok(new { items });
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyBrandDiscountRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var slug = NormalizeSlug(req.BrandSlug);
        if (string.IsNullOrWhiteSpace(slug)) return BadRequest(new { message = "Brand is required" });

        var brand = await _db.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Slug.ToLower() == slug);
        if (brand == null) return NotFound(new { message = "Brand not found" });

        var discount = Math.Clamp(req.DiscountPercent, 0, 100);
        var query = _db.Products.Where(p => p.Brand.ToLower() == slug);
        if (req.PublishedOnly) query = query.Where(p => p.IsPublished);

        var products = await query.ToListAsync();
        if (products.Count == 0)
        {
            return BadRequest(new { message = "This brand has no products to update" });
        }

        foreach (var p in products)
        {
            p.DiscountPercent = discount;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = discount > 0 ? "Brand discount applied" : "Brand discount cleared",
            brand = brand.Slug,
            discountPercent = discount,
            affectedProducts = products.Count
        });
    }

    [HttpPost("clear")]
    public async Task<IActionResult> Clear([FromBody] ClearBrandDiscountRequest req)
    {
        var slug = NormalizeSlug(req.BrandSlug);
        if (string.IsNullOrWhiteSpace(slug)) return BadRequest(new { message = "Brand is required" });

        var exists = await _db.Brands.AsNoTracking().AnyAsync(b => b.Slug.ToLower() == slug);
        if (!exists) return NotFound(new { message = "Brand not found" });

        var products = await _db.Products
            .Where(p => p.Brand.ToLower() == slug && p.DiscountPercent > 0)
            .ToListAsync();

        foreach (var p in products)
        {
            p.DiscountPercent = 0;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Brand discount cleared",
            brand = slug,
            affectedProducts = products.Count
        });
    }

    private static string NormalizeSlug(string? value)
    {
        var v = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(v)) return string.Empty;
        v = System.Text.RegularExpressions.Regex.Replace(v, "[`'\"]+", string.Empty);
        v = System.Text.RegularExpressions.Regex.Replace(v, @"[^a-z0-9\u0600-\u06FF]+", "-");
        v = System.Text.RegularExpressions.Regex.Replace(v, @"-+", "-").Trim('-');
        return v;
    }
}

public class ApplyBrandDiscountRequest
{
    [Required]
    public string BrandSlug { get; set; } = "";

    [Range(0, 100)]
    public int DiscountPercent { get; set; }

    public bool PublishedOnly { get; set; } = false;
}

public class ClearBrandDiscountRequest
{
    [Required]
    public string BrandSlug { get; set; } = "";
}
