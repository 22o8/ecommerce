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
                p.Brand,
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

        if (!BrandCatalog.IsAllowed(req.Brand))
            return BadRequest(new { message = "Invalid brand" });

        if (!BrandCatalog.IsAllowed(req.Brand))
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
            PriceUsd = req.PriceUsd,
            IsPublished = req.IsPublished,
            Brand = BrandCatalog.Normalize(req.Brand),
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
        p.PriceUsd = req.PriceUsd;
        p.IsPublished = req.IsPublished;
        p.Brand = BrandCatalog.Normalize(req.Brand);

        await _db.SaveChangesAsync();
        return Ok(new { message = "Updated" });
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
    public async Task<IActionResult> UploadImages(
        [FromRoute] Guid id,
        [FromForm(Name = "images")] List<IFormFile>? images,
        [FromForm(Name = "file")] IFormFile? file,
        [FromForm] string? alt
    )
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (product == null) return NotFound(new { message = "Product not found" });

        var files = new List<IFormFile>();
        if (images is { Count: > 0 }) files.AddRange(images);
        if (file != null && file.Length > 0) files.Add(file);

        if (files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var dir = Path.Combine(webRoot, "uploads", "products", id.ToString());
        Directory.CreateDirectory(dir);

        var maxOrder = await _db.ProductImages
            .Where(i => i.ProductId == id)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync() ?? 0;

        var created = new List<object>();

        foreach (var f in files)
        {
            var ext = Path.GetExtension(f.FileName);
            if (!allowed.Contains(ext))
                return BadRequest(new { message = $"File type not allowed: {ext}" });

            var safeName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(dir, safeName);

            await using (var fs = System.IO.File.Create(fullPath))
            {
                await f.CopyToAsync(fs);
            }

            // ✅ رابط كامل حتى يشتغل بكل مكان
            var publicPath = $"/uploads/products/{id}/{safeName}";
            var publicUrl = $"{Request.Scheme}://{Request.Host}{publicPath}";

            var img = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                Url = publicUrl,
                Alt = string.IsNullOrWhiteSpace(alt)
                    ? Path.GetFileNameWithoutExtension(f.FileName)
                    : alt.Trim(),
                SortOrder = ++maxOrder,
                CreatedAt = DateTime.UtcNow
            };

            _db.ProductImages.Add(img);
            created.Add(new { img.Id, img.Url, img.Alt, img.SortOrder });
        }

        await _db.SaveChangesAsync();
        return Ok(new { items = created });
    }

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
            if (!set.Contains(imgId))
                return BadRequest(new { message = $"ImageId {imgId} not found for this product" });

        for (int idx = 0; idx < req.ImageIds.Count; idx++)
        {
            var imgId = req.ImageIds[idx];
            var img = images.First(x => x.Id == imgId);
            img.SortOrder = idx + 1;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Reordered" });
    }

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

    private static string NormalizeSlug(string? slug)
        => (slug ?? "").Trim().ToLowerInvariant();

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
        while (s.Contains("--")) s = s.Replace("--", "-");
        s = s.Trim('-');

        return string.IsNullOrWhiteSpace(s) ? "item" : s;
    }

    private void TryDeletePhysicalFile(string? url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            // اذا رابط كامل (https://..) نحوله لمسار
            string relative;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                relative = uri.AbsolutePath.TrimStart('/');
            else
                relative = url.TrimStart('/');

            var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot",
                relative.Replace('/', Path.DirectorySeparatorChar));

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

    [Required]
    [MinLength(1)]
    public string Brand { get; set; } = "Unspecified";

    public bool IsPublished { get; set; }
}

public class ReorderImagesRequest
{
    [Required]
    public List<Guid> ImageIds { get; set; } = new();
}
