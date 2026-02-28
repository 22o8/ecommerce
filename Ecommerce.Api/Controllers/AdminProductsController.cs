using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Ecommerce.Api.Domain.Entities;
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

    // ============================
    // CRUD (Admin)
    // ============================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var items = await _db.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.PriceIqd,
                    p.PriceUsd,
                    p.IsPublished,
                    p.IsFeatured,
                    p.Brand,
                    p.CreatedAt,
                    imagesCount = p.Images.Count()
                })
                .ToListAsync();

            return Ok(items);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceIqd,
                x.PriceUsd,
                x.IsPublished,
                x.IsFeatured,
                x.Brand,
                x.CreatedAt,
                x.RatingAvg,
                x.RatingCount,
                images = x.Images
                    .OrderBy(i => i.SortOrder)
                    .ThenBy(i => i.CreatedAt)
                    .Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (p == null) return NotFound(new { message = "Product not found" });
        return Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertProductRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var brandSlug = NormalizeSlug(req.Brand);
        if (string.IsNullOrWhiteSpace(brandSlug))
            return BadRequest(new { message = "Brand is required" });

        var brandOk = await _db.Brands.AsNoTracking().AnyAsync(b => b.IsActive && b.Slug.ToLower() == brandSlug);
        if (!brandOk)
            return BadRequest(new { message = "Invalid brand" });

        var slug = NormalizeSlug(req.Slug);
        if (string.IsNullOrWhiteSpace(slug))
            slug = Slugify(req.Title);

        var exists = await _db.Products.AnyAsync(x => x.Slug.ToLower() == slug);
        if (exists) return BadRequest(new { message = "Slug already exists" });

        var p = new Product
        {
            Id = Guid.NewGuid(),
            Title = req.Title.Trim(),
            Slug = slug,
            Description = (req.Description ?? "").Trim(),
            PriceIqd = (req.PriceIqd > 0 ? req.PriceIqd : req.PriceUsd),
            PriceUsd = req.PriceUsd,
            IsPublished = req.IsPublished,
            IsFeatured = req.IsFeatured,
            Brand = brandSlug,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(p);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = p.Id }, new { p.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpsertProductRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound(new { message = "Product not found" });

        var slug = NormalizeSlug(req.Slug);
        if (string.IsNullOrWhiteSpace(slug))
            slug = Slugify(req.Title);

        var exists = await _db.Products.AnyAsync(x => x.Id != id && x.Slug.ToLower() == slug);
        if (exists) return BadRequest(new { message = "Slug already exists" });

        var brandSlug = NormalizeSlug(req.Brand);
        if (string.IsNullOrWhiteSpace(brandSlug))
            return BadRequest(new { message = "Brand is required" });

        var brandOk = await _db.Brands.AsNoTracking().AnyAsync(b => b.IsActive && b.Slug.ToLower() == brandSlug);
        if (!brandOk)
            return BadRequest(new { message = "Invalid brand" });

        p.Title = req.Title.Trim();
        p.Slug = slug;
        p.Description = (req.Description ?? "").Trim();
        p.PriceIqd = (req.PriceIqd > 0 ? req.PriceIqd : req.PriceUsd);
        p.PriceUsd = req.PriceUsd;
        p.IsPublished = req.IsPublished;
        p.IsFeatured = req.IsFeatured;
        p.Brand = brandSlug;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    [HttpPatch("{id:guid}/featured")]
    public async Task<IActionResult> SetFeatured([FromRoute] Guid id, [FromBody] SetFeaturedRequest req)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound(new { message = "Product not found" });

        p.IsFeatured = req.IsFeatured;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Updated", id = p.Id, p.IsFeatured });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        var hasOrders = await _db.OrderItems.AnyAsync(o => o.ProductId == id);
        if (hasOrders)
        {
            return BadRequest(new
            {
                message = "Cannot delete product because it has related orders. Unpublish it instead."
            });
        }

        if (product.Images?.Any() == true)
            _db.ProductImages.RemoveRange(product.Images);

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Deleted successfully" });
    }

    // ============================
    // Images (Admin)
    // ============================

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages([FromRoute] Guid id)
    {
        var exists = await _db.Products.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });

        var items = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == id)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder })
            .ToListAsync();

        return Ok(new { items });
    }

    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> UploadImages([FromRoute] Guid id, [FromForm] List<IFormFile>? files, [FromForm] string? alt = null)
    {
        // Some clients send field name "images" instead of "files".
        // To be resilient, fall back to reading any multipart files from the request.
        files ??= new List<IFormFile>();
        if (files.Count == 0 && Request.HasFormContentType)
        {
            foreach (var f in Request.Form.Files)
                files.Add(f);
        }

        if (files.Count == 0)
            return BadRequest(new { message = "No files uploaded" });

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound(new { message = "Product not found" });

        var currentMaxSort = await _db.ProductImages
            .Where(x => x.ProductId == id)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync();

        var sort = currentMaxSort ?? 0;
        var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        var created = new List<object>();

        foreach (var file in files)
        {
            if (file is null || file.Length <= 0) continue;

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext))
                return BadRequest(new { message = "Invalid file type. Allowed: jpg, jpeg, png, webp" });

            var key = $"products/{id}/{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

            await using var stream = file.OpenReadStream();
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;

            var upload = await _storage.UploadAsync(stream, key, contentType, HttpContext.RequestAborted);

            sort += 1;
            var img = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                Url = upload.Url,
                Alt = alt,
                SortOrder = sort,
                CreatedAt = DateTime.UtcNow
            };

            _db.ProductImages.Add(img);
            created.Add(new { img.Id, img.Url, img.Alt, img.SortOrder });
        }

        await _db.SaveChangesAsync();
        return Ok(new { items = created });
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage([FromRoute] Guid id, [FromRoute] Guid imageId)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.ProductId == id && x.Id == imageId);
        if (img is null) return NotFound(new { message = "Image not found" });

        var key = ExtractStorageKeyFromUrl(img.Url);
        if (!string.IsNullOrWhiteSpace(key))
            await _storage.DeleteAsync(key, HttpContext.RequestAborted);

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ============================
    // Helpers
    // ============================

    private static string NormalizeSlug(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        return Slugify(input);
    }

    private static string Slugify(string s)
    {
        s = s.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, @"[^a-z0-9\-]", "");
        s = Regex.Replace(s, @"-+", "-");
        return s.Trim('-');
    }

    private static string ExtractStorageKeyFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        try
        {
            var u = new Uri(url);
            return u.AbsolutePath.TrimStart('/');
        }
        catch
        {
            return url.TrimStart('/');
        }
    }
}

public class UpsertProductRequest
{
    [Required]
    [MinLength(2)]
    public string Title { get; set; } = "";

    public string? Slug { get; set; }
    public string? Description { get; set; }

    [Range(0, 999999999)]
    public decimal PriceIqd { get; set; }

    [Range(0, 999999)]
    public decimal PriceUsd { get; set; }

    [Required]
    [MinLength(1)]
    public string Brand { get; set; } = "Unspecified";

    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
}

public class SetFeaturedRequest
{
    public bool IsFeatured { get; set; }
}
