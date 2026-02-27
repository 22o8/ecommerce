using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
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
    private readonly IWebHostEnvironment _env;
    private readonly Ecommerce.Api.Infrastructure.Storage.IObjectStorage _storage;

    public AdminProductsController(AppDbContext db, IWebHostEnvironment env, Ecommerce.Api.Infrastructure.Storage.IObjectStorage storage)
    {
        _db = db;
        _env = env;
    }

    // ============================
    // CRUD (Admin)
    // ============================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // NOTE:
            // We intentionally avoid projecting navigation property counts directly (p.Images.Count)
            // because some provider/schema states can surface translation/runtime issues in production.
            // A correlated subquery is more robust.
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
                    imagesCount = _db.ProductImages.Count(i => i.ProductId == p.Id)
                })
                .ToListAsync();

            return Ok(items);
        }
        catch (Exception ex)
        {
            // Keep payload small but helpful; the traceId in the hosting platform logs is still the main reference.
            return StatusCode(500, new
            {
                error = "Internal Server Error",
                message = ex.Message
            });
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
                images = _db.ProductImages
                    .Where(i => i.ProductId == x.Id)
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

        p.Title = req.Title.Trim();
        p.Slug = slug;
        p.Description = (req.Description ?? "").Trim();
        p.PriceIqd = (req.PriceIqd > 0 ? req.PriceIqd : req.PriceUsd);
        p.PriceUsd = req.PriceUsd;
        p.IsPublished = req.IsPublished;
        p.IsFeatured = req.IsFeatured;
        var brandSlug = NormalizeSlug(req.Brand);
        if (string.IsNullOrWhiteSpace(brandSlug))
            return BadRequest(new { message = "Brand is required" });

        var brandOk = await _db.Brands.AsNoTracking().AnyAsync(b => b.IsActive && b.Slug.ToLower() == brandSlug);
        if (!brandOk)
            return BadRequest(new { message = "Invalid brand" });

        p.Brand = brandSlug;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    // ✅ Quick toggle featured without resending the full product payload
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

    // إذا المنتج مرتبط بطلبات → امنع الحذف
    var hasOrders = await _db.OrderItems.AnyAsync(o => o.ProductId == id);
    if (hasOrders)
    {
        return BadRequest(new
        {
            message = "Cannot delete product because it has related orders. Unpublish it instead."
        });
    }

    // حذف الصور من قاعدة البيانات فقط (بدون لمس الملفات)
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

        // خليها {items} حتى تكون ثابتة للفرونت
        return Ok(new { items });
    }

    // ✅ يقبل:
    // - images (متعدد)  => الفرونت الحالي عندك
    // - file   (مفرد)   => اذا اكو مكان ثاني يرسل file
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(30_000_000)]
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadImages([FromRoute] Guid id, [FromForm] List<IFormFile> files, [FromForm] string? alt = null)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded" });

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        // اجلب آخر ترتيب صور حتى نكمّل بعده
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

            // مفتاح التخزين داخل الـ bucket
            var key = $"products/{id}/{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

            await using var stream = file.OpenReadStream();
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;

            // لاحظ: UploadAsync يستقبل (content, key, contentType, ct)
            var upload = await _storage.UploadAsync(stream, key, contentType, HttpContext.RequestAborted);

            sort += 1;
            var img = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                StorageKey = upload.Key,
                Url = upload.Url,
                Alt = alt,
                SortOrder = sort,
                CreatedAt = DateTimeOffset.UtcNow
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
        if (img is null) return NotFound();

        await _storage.DeleteAsync(img.StorageKey, HttpContext.RequestAborted);
        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static string Slugify(string s)
    {
        s = s.Trim().ToLowerInvariant();
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", "-");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\-]", "");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"-+", "-");
        return s.Trim('-');
    }

    private static long ToIqd(long? v) => v ?? 0;

}

// Requests
// ============================

public class UpsertProductRequest
{
    [Required]
    [MinLength(2)]
    public string Title { get; set; } = "";

    public string? Slug { get; set; }
    public string? Description { get; set; }

    // ✅ المعتمد الآن: السعر بالدينار العراقي
    [Range(0, 999999999)]
    public decimal PriceIqd { get; set; }

    [Range(0, 999999)]
    public decimal PriceUsd { get; set; }

    [Required]
    [MinLength(1)]
    public string Brand { get; set; } = "Unspecified";

    public bool IsPublished { get; set; }

    // Show on home page (Featured Products)
    public bool IsFeatured { get; set; }
}

public class SetFeaturedRequest
{
    public bool IsFeatured { get; set; }
}

public class ReorderImagesRequest
{
    [Required]
    public List<Guid> ImageIds { get; set; } = new();
}
