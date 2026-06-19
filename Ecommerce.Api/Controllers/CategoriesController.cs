using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("active")]
    public async Task<IActionResult> Active([FromQuery] string? section = null, [FromQuery] Guid? parentId = null, [FromQuery] bool rootsOnly = false)
    {
        var normalizedSection = string.IsNullOrWhiteSpace(section) ? "regular" : section.Trim().ToLowerInvariant();
        var query = _db.Categories
            .AsNoTracking()
            .Where(x => x.IsActive && x.Section.ToLower() == normalizedSection);

        if (parentId.HasValue)
            query = query.Where(x => x.ParentId == parentId.Value);
        else if (rootsOnly)
            query = query.Where(x => x.ParentId == null);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameAr)
            .Select(x => new
            {
                x.Id,
                key = x.Key,
                nameAr = x.NameAr,
                nameEn = x.NameEn,
                descriptionAr = x.DescriptionAr,
                descriptionEn = x.DescriptionEn,
                imageUrl = x.ImageUrl,
                section = x.Section,
                x.ParentId,
                x.HasDetailSections,
                childCount = _db.Categories.Count(c => c.IsActive && c.ParentId == x.Id),
                x.SortOrder,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }
}
