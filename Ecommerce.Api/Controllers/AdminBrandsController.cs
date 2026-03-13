// Ecommerce.Api/Controllers/AdminBrandsController.cs
using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Storage;
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
    private readonly IObjectStorage _storage;

    public AdminBrandsController(AppDbContext db, IWebHostEnvironment env, IObjectStorage storage)
    {
        _db = db;
        _env = env;
        _storage = storage;
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

        var lower = slug.ToLowerInvariant();
        var exists = await _db.Brands.AnyAsync(x => x.Slug.ToLower() == lower);
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

        var lower = slug.ToLowerInvariant();
        var exists = await _db.Brands.AnyAsync(x => x.Id != id && x.Slug.ToLower() == lower);
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

        var brandSlug = (b.Slug ?? "").Trim().ToLowerInvariant();

        var hasProducts = await _db.Products.AnyAsync(p =>
            (p.Brand ?? "").ToLower() == brandSlug
        );

        if (hasProducts)
            return BadRequest(new { message = "Cannot delete brand because it has products. Disable it instead." });

        _db.Brands.Remove(b);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Deleted" });
    }

    // Upload square logo
    [HttpPost("{id:guid}/logo")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadLogo([FromRoute] Guid id, [FromForm] IFormFile? file)
    {
        var b = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Brand not found" });

        // Some clients send the field as "logo" or any other name.
        if ((file == null || file.Length == 0) && Request.HasFormContentType && Request.Form.Files.Count > 0)
            file = Request.Form.Files[0];

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext))
            return BadRequest(new { message = $"File type not allowed: {ext}" });

        // Use the same storage pipeline as product images so logos are visible consistently on all devices.
        // This also keeps brand images compatible with local storage or S3/R2 without extra per-device logic.
        var oldKey = ExtractStorageKeyFromUrl(b.LogoUrl);
        var key = $"brands/{id}/logo{ext.ToLowerInvariant()}";

        await using (var stream = file.OpenReadStream())
        {
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
            var upload = await _storage.UploadAsync(stream, key, contentType, HttpContext.RequestAborted);
            b.LogoUrl = upload.Url;
        }

        if (!string.IsNullOrWhiteSpace(oldKey) && !string.Equals(oldKey, key, StringComparison.OrdinalIgnoreCase))
        {
            try { await _storage.DeleteAsync(oldKey, HttpContext.RequestAborted); } catch { }
        }

        await _db.SaveChangesAsync();

        return Ok(new { logoUrl = b.LogoUrl, url = b.LogoUrl, key });
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


    private static string ExtractStorageKeyFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;

        try
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var u = new Uri(url);
                var path = u.AbsolutePath ?? string.Empty;
                if (path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                    return path.Substring("/uploads/".Length).Trim('/');
                return string.Empty;
            }

            if (url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                return url.Substring("/uploads/".Length).Trim('/');
        }
        catch { }

        return string.Empty;
    }

    private static string NormalizeSlug(string? s)
    {
        s = (s ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;

        s = System.Text.RegularExpressions.Regex.Replace(s, "[`'\"]+", string.Empty);
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\u0600-\u06FF]+", "-");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"-+", "-").Trim('-');

        return s;
    }
}
