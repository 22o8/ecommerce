using System.Text.RegularExpressions;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public AdminCategoriesController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public record SaveCategoryRequest(
        string Key,
        string NameAr,
        string? NameEn,
        string? DescriptionAr,
        string? DescriptionEn,
        string? ImageUrl,
        int SortOrder,
        bool IsActive
    );

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.Categories
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameAr)
            .ToListAsync();

        return Ok(items.Select(x => new
        {
            x.Id,
            x.Key,
            x.NameAr,
            x.NameEn,
            x.DescriptionAr,
            x.DescriptionEn,
            x.ImageUrl,
            x.SortOrder,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveCategoryRequest req)
    {
        var key = Slugify(req.Key);
        if (string.IsNullOrWhiteSpace(key)) key = Slugify(req.NameEn) ?? Slugify(req.NameAr);
        if (string.IsNullOrWhiteSpace(key)) return BadRequest(new { message = "Category key is required" });
        if (string.IsNullOrWhiteSpace(req.NameAr)) return BadRequest(new { message = "Arabic name is required" });
        if (await _db.Categories.AnyAsync(x => x.Key.ToLower() == key))
            return BadRequest(new { message = "Category already exists" });

        var entity = new CategoryDefinition
        {
            Id = Guid.NewGuid(),
            Key = key,
            NameAr = req.NameAr.Trim(),
            NameEn = req.NameEn?.Trim(),
            DescriptionAr = req.DescriptionAr?.Trim(),
            DescriptionEn = req.DescriptionEn?.Trim(),
            ImageUrl = req.ImageUrl?.Trim(),
            SortOrder = req.SortOrder,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(new { entity.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveCategoryRequest req)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        var key = Slugify(req.Key);
        if (string.IsNullOrWhiteSpace(key)) key = entity.Key;
        if (await _db.Categories.AnyAsync(x => x.Id != id && x.Key.ToLower() == key))
            return BadRequest(new { message = "Category already exists" });

        entity.Key = key;
        entity.NameAr = string.IsNullOrWhiteSpace(req.NameAr) ? entity.NameAr : req.NameAr.Trim();
        entity.NameEn = req.NameEn?.Trim();
        entity.DescriptionAr = req.DescriptionAr?.Trim();
        entity.DescriptionEn = req.DescriptionEn?.Trim();
        entity.ImageUrl = req.ImageUrl?.Trim();
        entity.SortOrder = req.SortOrder;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { entity.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<object>> Upload([FromForm] IFormFile? file)
    {
        if ((file is null || file.Length == 0) && Request.HasFormContentType && Request.Form.Files.Count > 0)
            file = Request.Form.Files[0];

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var id = Guid.NewGuid();
        var key = $"uploads/categories/{id}{ext}";

        await using var stream = file.OpenReadStream();
        var stored = await _storage.UploadAsync(stream, key, file.ContentType);

        return Ok(new { url = stored.Url, key = stored.Key });
    }

    private static string Slugify(string? value)
    {
        var s = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = Regex.Replace(s, "[^a-z0-9\\s_-]", string.Empty);
        s = Regex.Replace(s, "[\\s_]+", "-").Trim('-');
        return s;
    }
}
