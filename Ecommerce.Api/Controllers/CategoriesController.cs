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
    public async Task<IActionResult> Active()
    {
        var items = await _db.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
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
                x.SortOrder,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }
}
