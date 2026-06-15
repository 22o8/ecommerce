using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/packages")]
[AllowAnonymous]
public class PackagesController : ControllerBase
{
    private readonly AppDbContext _db;
    public PackagesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? placement = null)
    {
        var q = _db.ProductPackages.AsNoTracking().Include(x => x.Items).ThenInclude(i => i.Product).Where(x => x.IsPublished);
        if (!string.IsNullOrWhiteSpace(placement)) q = q.Where(x => x.ShowInSlider && x.SliderPlacement == placement.Trim());
        var items = await q.OrderBy(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(items.Select(AdminPackagesController.ToDto));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        var p = await _db.ProductPackages.AsNoTracking().Include(x => x.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(x => x.IsPublished && x.Slug == slug);
        return p == null ? NotFound() : Ok(AdminPackagesController.ToDto(p));
    }
}
