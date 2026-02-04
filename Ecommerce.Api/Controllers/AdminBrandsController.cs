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
    private readonly IWebHostEnvironment _env;

    public AdminBrandsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
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
                b.Slug,
                b.Name,
                b.Description,
                b.LogoUrl,
                b.IsActive,
                b.CreatedAt
            })
            .ToListAsync();

        return Ok(new { items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var b = await _db.Brands.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Brand not found" });
        return Ok(b);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertBrandRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var slug = NormalizeSlug(req.Slug);
        if (string.IsNullOrWhiteSpace(slug)) slug = NormalizeSlug(req.Name);

        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest(new { message = "Slug is required" });

        var exists = await _db.Brands.AnyAsync(x => x.Slug.ToLower() == slug);
        if (exists) return BadRequest(new { message = "Slug already exists" });

        var b = new Brand
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Name = req.Name.Trim(),
            Description = (req.Description ?? "").Trim(),
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Brands.Add(b);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = b.Id }, new { b.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpsertBrandRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var b = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Brand not found" });

        var slug = NormalizeSlug(req.Slug);
        if (string.IsNullOrWhiteSpace(slug)) slug = NormalizeSlug(req.Name);

        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest(new { message = "Slug is required" });

        var exists = await _db.Brands.AnyAsync(x => x.Id != id && x.Slug.ToLower() == slug);
        if (exists) return BadRequest(new { message = "Slug already exists" });

        b.Slug = slug;
        b.Name = req.Name.Trim();
        b.Description = (req.Description ?? "").Trim();
        b.IsActive = req.IsActive;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var b = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Brand not found" });

        var hasProducts = await _db.Products.AnyAsync(p => p.Brand.ToLower() == b.Slug.ToLower());
        if (hasProducts)
            return BadRequest(new { message = "Cannot delete brand because it has products. Disable it instead." });

        _db.Brands.Remove(b);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Deleted" });
    }

    // Upload square logo
    [HttpPost("{id:guid}/logo")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadLogo([FromRoute] Guid id, [FromForm] IFormFile file)
    {
        var b = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Brand not found" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

        var ext = Path.GetExtension(file.FileName);
        if (!allowed.Contains(ext))
            return BadRequest(new { message = $"File type not allowed: {ext}" });

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var dir = Path.Combine(webRoot, "uploads", "brands", id.ToString());
        Directory.CreateDirectory(dir);

        var fileName = $"logo{ext.ToLowerInvariant()}";

        var absPath = Path.Combine(dir, fileName);
        await using (var fs = new FileStream(absPath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        b.LogoUrl = $"/uploads/brands/{id}/{fileName}";
        await _db.SaveChangesAsync();

        return Ok(new { logoUrl = b.LogoUrl });
    }

    public class UpsertBrandRequest
    {
        [Required, MaxLength(120)]
        public string Name { get; set; } = "";

        [MaxLength(80)]
        public string? Slug { get; set; }

        [MaxLength(400)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    private static string NormalizeSlug(string? s)
    {
        s = (s ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", "-");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\-]", "");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"-+", "-").Trim('-');
        return s;
    }
}
