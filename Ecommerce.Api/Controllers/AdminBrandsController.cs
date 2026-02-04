using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/brands")]
[Authorize(Roles = "Admin")]
public class AdminBrandsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminBrandsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Slug,
                b.LogoUrl,
                b.BannerUrl,
                b.Description,
                b.IsActive,
                b.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertBrandRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required");

        var name = req.Name.Trim();
        var slug = string.IsNullOrWhiteSpace(req.Slug)
            ? Slugify(name)
            : Slugify(req.Slug);

        var exists = await _db.Brands.AnyAsync(x => x.Slug == slug);
        if (exists)
            return Conflict("Slug already exists");

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(req.LogoUrl) ? null : req.LogoUrl.Trim(),
            BannerUrl = string.IsNullOrWhiteSpace(req.BannerUrl) ? null : req.BannerUrl.Trim(),
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();

        return Ok(brand);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertBrandRequest req)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Name))
            brand.Name = req.Name.Trim();

        if (!string.IsNullOrWhiteSpace(req.Slug))
        {
            var slug = Slugify(req.Slug);
            var exists = await _db.Brands.AnyAsync(x => x.Slug == slug && x.Id != id);
            if (exists) return Conflict("Slug already exists");
            brand.Slug = slug;
        }

        brand.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        brand.LogoUrl = string.IsNullOrWhiteSpace(req.LogoUrl) ? null : req.LogoUrl.Trim();
        brand.BannerUrl = string.IsNullOrWhiteSpace(req.BannerUrl) ? null : req.BannerUrl.Trim();
        brand.IsActive = req.IsActive;

        await _db.SaveChangesAsync();
        return Ok(brand);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand is null) return NotFound();

        _db.Brands.Remove(brand);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static string Slugify(string text)
    {
        text = text.Trim().ToLowerInvariant();
        // تبسيط سريع
        var chars = text.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}

public class UpsertBrandRequest
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Slug { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? LogoUrl { get; set; }

    [MaxLength(2000)]
    public string? BannerUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
