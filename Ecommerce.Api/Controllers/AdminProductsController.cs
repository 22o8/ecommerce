using System.Globalization;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public AdminProductsController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    // -----------------------------
    // Products CRUD
    // -----------------------------

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var q = _db.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .OrderByDescending(p => p.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                p.Description,
                p.IsActive,
                p.IsFeatured,
                Brand = p.Brand == null ? null : new { p.Brand.Id, p.Brand.Name, p.Brand.Slug },
                p.CreatedAt,
                p.UpdatedAt
            })
            .ToListAsync();

        return Ok(new { page, pageSize, total, items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Include(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound();

        return Ok(new
        {
            p.Id,
            p.Name,
            p.Slug,
            p.Price,
            p.Description,
            p.IsActive,
            p.IsFeatured,
            Brand = p.Brand == null ? null : new { p.Brand.Id, p.Brand.Name, p.Brand.Slug },
            p.CreatedAt,
            p.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertProductRequest req)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == req.BrandId);
        if (brand == null) return BadRequest(new { message = "Brand not found" });

        var now = DateTimeOffset.UtcNow;

        var p = new Ecommerce.Api.Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = (req.Name ?? "").Trim(),
            Slug = BuildSlug(req.Slug, req.Name),
            Description = req.Description,
            Price = NormalizePrice(req.Price),
            BrandId = req.BrandId,
            IsActive = req.IsActive,
            IsFeatured = req.IsFeatured,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Products.Add(p);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = p.Id }, new { p.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertProductRequest req)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == req.BrandId);
        if (brand == null) return BadRequest(new { message = "Brand not found" });

        p.Name = (req.Name ?? "").Trim();
        p.Slug = BuildSlug(req.Slug, req.Name);
        p.Description = req.Description;
        p.Price = NormalizePrice(req.Price);
        p.BrandId = req.BrandId;
        p.IsActive = req.IsActive;
        p.IsFeatured = req.IsFeatured;
        p.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { p.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _db.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound();

        // best-effort delete images from storage
        if (p.Images != null)
        {
            foreach (var img in p.Images)
            {
                var key = !string.IsNullOrWhiteSpace(img.StorageKey) ? img.StorageKey : ExtractStorageKeyFromUrl(img.Url);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    try { await _storage.DeleteAsync(key); } catch { /* ignore */ }
                }
            }
        }

        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // -----------------------------
    // Images (R2 / S3)
    // -----------------------------

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(Guid id)
    {
        var exists = await _db.Products.AsNoTracking().AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();

        var images = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == id)
            .OrderBy(i => i.Order)
            .ThenBy(i => i.CreatedAt)
            .Select(i => new
            {
                i.Id,
                i.ProductId,
                i.Url,
                i.StorageKey,
                i.Order,
                i.CreatedAt
            })
            .ToListAsync();

        return Ok(images);
    }

    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> UploadImages(Guid id, [FromForm] IFormFileCollection files)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();
        if (files == null || files.Count == 0) return BadRequest(new { message = "No files" });

        var maxOrder = await _db.ProductImages
            .Where(x => x.ProductId == id)
            .MaxAsync(x => (int?)x.Order) ?? 0;

        var created = new List<object>();

        foreach (var file in files)
        {
            if (file.Length <= 0) continue;

            // Accept any image (even if content-type is missing) but block absurdly large single files
            if (file.Length > 20_000_000)
                return BadRequest(new { message = "File too large (max 20MB each)" });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = GuessExtension(file.ContentType);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";
            ext = ext.StartsWith('.') ? ext : "." + ext;

            var key = $"products/{id:D}/{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

            await using var stream = file.OpenReadStream();
            var url = await _storage.UploadAsync(key, stream, file.ContentType);

            var img = new Ecommerce.Api.Domain.Entities.ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                Url = url,
                StorageKey = key,
                Order = ++maxOrder,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.ProductImages.Add(img);
            created.Add(new { img.Id, img.Url, img.StorageKey, img.Order });
        }

        await _db.SaveChangesAsync();
        return Ok(created);
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == id);
        if (img == null) return NotFound();

        var key = !string.IsNullOrWhiteSpace(img.StorageKey) ? img.StorageKey : ExtractStorageKeyFromUrl(img.Url);
        if (!string.IsNullOrWhiteSpace(key))
        {
            try { await _storage.DeleteAsync(key); } catch { /* ignore */ }
        }

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // -----------------------------
    // Helpers
    // -----------------------------

    private static string BuildSlug(string? slug, string? name)
    {
        var baseText = string.IsNullOrWhiteSpace(slug) ? (name ?? "") : slug;
        baseText = baseText.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(baseText)) return Guid.NewGuid().ToString("N");

        // very simple slugify (latin + numbers + dashes)
        var chars = baseText
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsLetterOrDigit(c) ? c : '-');

        var cleaned = new string(chars.ToArray());
        while (cleaned.Contains("--")) cleaned = cleaned.Replace("--", "-");
        cleaned = cleaned.Trim('-');
        return string.IsNullOrWhiteSpace(cleaned) ? Guid.NewGuid().ToString("N") : cleaned;
    }

    private static decimal NormalizePrice(decimal price)
    {
        if (price < 0) price = 0;
        return decimal.Round(price, 2);
    }

    private static string? GuessExtension(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return null;
        contentType = contentType.ToLowerInvariant();
        return contentType switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/svg+xml" => ".svg",
            "image/avif" => ".avif",
            _ => null
        };
    }

    private static string? ExtractStorageKeyFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
        var path = uri.AbsolutePath.TrimStart('/');
        // some endpoints might include bucket name in path (s3 style): /bucket/key
        // our public URLs (r2.dev / custom domain) usually are just /key
        if (path.StartsWith("ecommerce-images/", StringComparison.OrdinalIgnoreCase))
            path = path.Substring("ecommerce-images/".Length);
        return string.IsNullOrWhiteSpace(path) ? null : path;
    }
}

public class UpsertProductRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid BrandId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
}
