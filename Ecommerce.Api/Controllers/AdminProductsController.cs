using System.Text;
using System.Globalization;
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

    // -------------------------
    // LIST
    // -------------------------
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(p =>
                p.Title.Contains(s) ||
                p.Slug.Contains(s) ||
                (p.Brand != null && p.Brand.Contains(s)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                title = p.Title,
                p.Slug,
                p.Brand,
                priceIqd = p.PriceIqd,
                p.Currency,
                isActive = p.IsPublished,
                p.IsFeatured,
                p.RatingAvg,
                p.RatingCount,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(new { page, pageSize, total, items });
    }

    // -------------------------
    // GET BY ID
    // -------------------------
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _db.Products
            .Include(x => x.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return NotFound();

        return Ok(new
        {
            p.Id,
            title = p.Title,
            p.Slug,
            p.Brand,
            priceIqd = p.PriceIqd,
            p.Currency,
            isActive = p.IsPublished,
            p.IsFeatured,
            p.RatingAvg,
            p.RatingCount,
            p.CreatedAt,
            images = p.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => new { i.Id, i.Url, sortOrder = i.SortOrder })
                .ToList()
        });
    }

    // -------------------------
    // CREATE / UPDATE
    // -------------------------
    public class UpsertProductRequest
    {
        public string Title { get; set; } = "";
        public string? Slug { get; set; }
        public string? Brand { get; set; }
        public decimal PriceIqd { get; set; }
        public string Currency { get; set; } = "IQD";
        public bool IsActive { get; set; } = true; // maps to IsPublished
        public bool IsFeatured { get; set; } = false;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertProductRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title)) return BadRequest("Title is required.");

        var slug = string.IsNullOrWhiteSpace(req.Slug) ? Slugify(req.Title) : Slugify(req.Slug);

        var p = new Product
        {
            Title = req.Title.Trim(),
            Slug = slug,
            Brand = string.IsNullOrWhiteSpace(req.Brand) ? null : req.Brand.Trim(),
            PriceIqd = req.PriceIqd,
            Currency = string.IsNullOrWhiteSpace(req.Currency) ? "IQD" : req.Currency.Trim().ToUpperInvariant(),
            IsPublished = req.IsActive,
            IsFeatured = req.IsFeatured,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.Id }, new { p.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertProductRequest req)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Title))
            p.Title = req.Title.Trim();

        if (!string.IsNullOrWhiteSpace(req.Slug))
            p.Slug = Slugify(req.Slug);
        else if (string.IsNullOrWhiteSpace(p.Slug))
            p.Slug = Slugify(p.Title);

        p.Brand = string.IsNullOrWhiteSpace(req.Brand) ? null : req.Brand.Trim();
        p.PriceIqd = req.PriceIqd;
        p.Currency = string.IsNullOrWhiteSpace(req.Currency) ? "IQD" : req.Currency.Trim().ToUpperInvariant();
        p.IsPublished = req.IsActive;
        p.IsFeatured = req.IsFeatured;

        await _db.SaveChangesAsync();
        return Ok(new { p.Id });
    }

    // -------------------------
    // IMAGES
    // -------------------------
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> UploadImages(Guid id, [FromForm] IFormFileCollection files)
    {
        var p = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        if (files == null || files.Count == 0) return BadRequest("No files.");

        // next sort order
        var nextOrder = p.Images.Count == 0 ? 1 : p.Images.Max(i => i.SortOrder) + 1;

        var uploaded = new List<object>();

        foreach (var file in files)
        {
            if (file.Length <= 0) continue;

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

            var key = $"products/{p.Id}/{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            await using var stream = file.OpenReadStream();

            var upload = await _storage.UploadAsync(
                key: key,
                content: stream,
                contentType: string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                cancellationToken: HttpContext.RequestAborted
            );

            var img = new ProductImage
            {
                ProductId = p.Id,
                Url = upload.Url,
                SortOrder = nextOrder++
            };

            p.Images.Add(img);

            uploaded.Add(new { img.Id, img.Url, sortOrder = img.SortOrder });
        }

        await _db.SaveChangesAsync();
        return Ok(new { uploaded });
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == id);
        if (img == null) return NotFound();

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();

        // ملاحظة: حذف الملف من التخزين اختياري. إذا تريد نحذفه أيضاً لازم نخزن الـ key.
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // -------------------------
    // Helpers
    // -------------------------
    private static string Slugify(string value)
    {
        value = (value ?? "").Trim();

        // normalize
        value = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc == System.Globalization.UnicodeCategory.NonSpacingMark) continue;

            // keep letters/digits; convert spaces to '-'
            if (char.IsLetterOrDigit(c))
                sb.Append(char.ToLowerInvariant(c));
            else if (char.IsWhiteSpace(c) || c == '-' || c == '_')
                sb.Append('-');
        }

        var slug = sb.ToString();
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        slug = slug.Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }
}
