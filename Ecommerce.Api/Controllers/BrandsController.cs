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

    // GET: /api/Brands?active=true
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool active = true)
    {
        var q = _db.Brands.AsNoTracking();
        if (active) q = q.Where(b => b.IsActive);

        var items = await q
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Slug,
                b.LogoUrl,
                b.BannerUrl,
                b.Description,
                b.IsActive
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: /api/Brands/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
        slug = (slug ?? string.Empty).Trim().ToLower();
        if (string.IsNullOrWhiteSpace(slug)) return BadRequest(new { message = "Invalid slug" });

        var brand = await _db.Brands.AsNoTracking()
            .Where(b => b.Slug.ToLower() == slug && b.IsActive)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Slug,
                b.LogoUrl,
                b.BannerUrl,
                b.Description,
                b.IsActive
            })
            .FirstOrDefaultAsync();

        if (brand == null) return NotFound(new { message = "Brand not found" });
        return Ok(brand);
    }
}
