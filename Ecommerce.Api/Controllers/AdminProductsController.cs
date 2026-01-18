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

    public AdminProductsController(AppDbContext db, IWebHostEnvironment env)
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
        var items = await _db.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.PriceUsd,
                p.IsPublished,
                p.CreatedAt,
                imagesCount = _db.ProductImages.Count(i => i.ProductId == p.Id),
            })
            .ToListAsync();

        return Ok(items);
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
                x.PriceUsd,
                x.IsPublished,
                x.CreatedAt,
                x.RatingAvg,
                x.RatingCount,
                images = _db.ProductImages
                    .Where(i => i.ProductId == x.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new
                    {
                        i.Id,
                        url = GetPublicUrl(i.Url), // ✅ رجع رابط صحيح للعرض
                        i.Alt,
                        i.SortOrder
                    })
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

        var slug = (req.Slug ?? "").Trim().ToLowerInvariant();
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
            PriceUsd = req.PriceUsd,
            IsPublished = req.IsPublished,
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

        var slug = (req.Slug ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(slug))
            slug = Slugify(req.Title);

        var exists = await _db.Products.AnyAsync(x => x.Id != id && x.Slug.ToLower() == slug);
        if (exists) return BadRequest(new { message = "Slug already exists" });

        p.Title = req.Title.Trim();
        p.Slug = slug;
        p.Description = (req.Description ?? "").Trim();
        p.PriceUsd = req.PriceUsd;
        p.IsPublished = req.IsPublished;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound(new { message = "Product not found" });

        // حذف الصور من قاعدة البيانات + حذف ملفاتها من السيرفر
        var images = await _db.ProductImages.Where(i => i.ProductId == id).ToListAsync();
        foreach (var img in images)
            TryDeletePhysicalFile(img.Url);

        _db.ProductImages.RemoveRange(images);
        _db.Products.Remove(p);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Deleted" });
    }

    // ============================
    // Images (Admin)
    // ============================

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages([FromRoute] Guid id)
    {
        var exists = await _db.Products.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });

        var images = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == id)
            .OrderBy(i => i.SortOrder)
            .Select(i => new
            {
                i.Id,
                url = GetPublicUrl(i.Url), // ✅ رجع رابط صحيح للعرض
                i.Alt,
                i.SortOrder
            })
            .ToListAsync();

        return Ok(images);
    }

    // رفع صورة: multipart/form-data
    // key = file
    // optional key = alt
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadImage([FromRoute] Guid id, IFormFile file, [FromForm] string? alt)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound(new { message = "Product not found" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext))
            return BadRequest(new { message = "Only jpg/jpeg/png/webp allowed" });

        // ✅ تأكد webroot موجود حتى داخل Render
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var uploadsRoot = Path.Combine(webRoot, "uploads", "products", id.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsRoot, safeName);

        await using (var fs = System.IO.File.Create(fullPath))
            await file.CopyToAsync(fs);

        // ترتيب الصورة: آخر شي
        var maxOrder = await _db.ProductImages
            .Where(i => i.ProductId == id)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync() ?? 0;

        // ✅ خزن مسار نسبي فقط (هذا أهم تعديل)
        var url = $"/uploads/products/{id}/{safeName}";

        var img = new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = id,
            Url = url, // ✅ نسبي
            Alt = (alt ?? "").Trim(),
            SortOrder = maxOrder + 1
        };

        _db.ProductImages.Add(img);
        await _db.SaveChangesAsync();

        // ✅ رجّع رابط كامل للعرض
        return Ok(new { img.Id, url = GetPublicUrl(img.Url), img.Alt, img.SortOrder });
    }

    // إعادة ترتيب الصور
    [HttpPut("{id:guid}/images/reorder")]
    public async Task<IActionResult> ReorderImages([FromRoute] Guid id, [FromBody] ReorderImagesRequest req)
    {
        if (req?.ImageIds == null || req.ImageIds.Count == 0)
            return BadRequest(new { message = "ImageIds is required" });

        var exists = await _db.Products.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });

        var images = await _db.ProductImages
            .Where(i => i.ProductId == id)
            .ToListAsync();

        var set = images.Select(i => i.Id).ToHashSet();
        foreach (var imgId in req.ImageIds)
        {
            if (!set.Contains(imgId))
                return BadRequest(new { message = $"ImageId {imgId} not found for this product" });
        }

        for (int idx = 0; idx < req.ImageIds.Count; idx++)
        {
            var imgId = req.ImageIds[idx];
            var img = images.First(x => x.Id == imgId);
            img.SortOrder = idx + 1;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Reordered" });
    }

    // حذف صورة
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage([FromRoute] Guid id, [FromRoute] Guid imageId)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(i => i.ProductId == id && i.Id == imageId);
        if (img == null) return NotFound(new { message = "Image not found" });

        TryDeletePhysicalFile(img.Url);

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Deleted" });
    }

    // ============================
    // Helpers
    // ============================

    private static string Slugify(string input)
    {
        input = (input ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(input)) return "item";

        var chars = input.Select(c =>
        {
            if (char.IsLetterOrDigit(c)) return c;
            if (char.IsWhiteSpace(c) || c == '-' || c == '_') return '-';
            return '-';
        }).ToArray();

        var s = new string(chars);

        while (s.Contains("--"))
            s = s.Replace("--", "-");

        s = s.Trim('-');
        return string.IsNullOrWhiteSpace(s) ? "item" : s;
    }

    // ✅ يبني رابط صحيح للصور حتى لو الطلب جاي من BFF/فرونت
    private string GetPublicUrl(string? urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath)) return "";

        // إذا مخزن رابط كامل خلّه كما هو
        if (urlOrPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            urlOrPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return urlOrPath;

        // مسار نسبي => رجّعه على دومين الـ API الفعلي
        var apiBase = $"{Request.Scheme}://{Request.Host}";
        return $"{apiBase}{(urlOrPath.StartsWith("/") ? "" : "/")}{urlOrPath}";
    }

    private void TryDeletePhysicalFile(string? url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            // إذا رابط كامل حوله لمسار
            var pathOnly = url;
            if (Uri.TryCreate(url, UriKind.Absolute, out var abs))
                pathOnly = abs.AbsolutePath;

            var relative = pathOnly.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

            var fullPath = Path.Combine(webRoot, relative);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
        catch
        {
            // تجاهل
        }
    }
}

// ============================
// Requests
// ============================

public class UpsertProductRequest
{
    [Required]
    [MinLength(2)]
    public string Title { get; set; } = "";

    public string? Slug { get; set; }
    public string? Description { get; set; }

    [Range(0, 999999)]
    public decimal PriceUsd { get; set; }

    public bool IsPublished { get; set; }
}

public class ReorderImagesRequest
{
    [Required]
    public List<Guid> ImageIds { get; set; } = new();
}
