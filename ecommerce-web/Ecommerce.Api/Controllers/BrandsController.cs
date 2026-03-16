// Ecommerce.Api/Controllers/BrandsController.cs
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BrandsController(AppDbContext db)
    {
        _db = db;
    }

    // Public brands list (for storefront)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Brands
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                id = b.Id,
                slug = b.Slug,
                name = b.Name,
                description = b.Description,
                logoUrl = b.LogoUrl
            })
            .ToListAsync();

        return Ok(new { items });
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
        slug = (slug ?? "").Trim();

        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest(new { message = "Slug is required" });

        var lower = slug.ToLowerInvariant();

        var b = await _db.Brands
            .AsNoTracking()
            .Where(x => x.IsActive && x.Slug.ToLower() == lower)
            .Select(x => new
            {
                id = x.Id,
                slug = x.Slug,
                name = x.Name,
                description = x.Description,
                logoUrl = x.LogoUrl
            })
            .FirstOrDefaultAsync();

        if (b == null) return NotFound(new { message = "Brand not found" });
        return Ok(b);
    }
}
