using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/services
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _db.Services.AsNoTracking()
            .Where(s => s.IsPublished)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Slug,
                s.Description,
                s.CreatedAt,
                Packages = s.Packages.Select(p => new { p.Id, p.Name, p.PriceUsd, p.DeliveryDays, p.Features }).ToList()
            })
            .ToListAsync();

        return Ok(services);
    }

    // GET /api/services/slug/{slug}
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var s = (slug ?? "").Trim();
        if (string.IsNullOrWhiteSpace(s)) return BadRequest("slug is required");

        var service = await _db.Services.AsNoTracking()
            .Where(x => x.IsPublished && x.Slug == s)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.CreatedAt,
                Packages = x.Packages.Select(p => new { p.Id, p.Name, p.PriceUsd, p.DeliveryDays, p.Features }).ToList(),
                Requirements = x.Requirements.OrderBy(r => r.Order).Select(r => new { r.Id, r.Question, r.IsRequired, r.Order }).ToList()
            })
            .FirstOrDefaultAsync();

        if (service == null) return NotFound();
        return Ok(service);
    }
}
