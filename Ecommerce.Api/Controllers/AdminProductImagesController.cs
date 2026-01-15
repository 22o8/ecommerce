// Ecommerce.Api/Controllers/AdminProductsController.cs
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminProductsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // ✅ GET images
    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages([FromRoute] Guid id)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!exists) return NotFound(new { message = "Product not found" });

        var items = await _db.ProductImages
            .Where(x => x.ProductId == id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new
            {
                id = x.Id,
                url = x.Url,
                alt = x.Alt,
                sortOrder = x.SortOrder
            })
            .ToListAsync();

        return Ok(new { items });
    }

    // ✅ POST upload images (multipart/form-data) field name: files
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(30_000_000)] // 30MB
    public async Task<IActionResult> UploadImages([FromRoute] Guid id, [FromForm] List<IFormFile> files)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound(new { message = "Product not found" });

        if (files == null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var dir = Path.Combine(webRoot, "uploads", "products", id.ToString());
        Directory.CreateDirectory(dir);

        var maxSort = await _db.ProductImages
            .Where(x => x.ProductId == id)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? -1;

        var created = new List<object>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!allowed.Contains(ext))
                return BadRequest(new { message = $"File type not allowed: {ext}" });

            var safeName = $"{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(dir, safeName);

            await using (var stream = System.IO.File.Create(full))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = $"/uploads/products/{id}/{safeName}";
var publicUrl = $"{Request.Scheme}://{Request.Host}{publicPath}";


            var img = new Ecommerce.Api.Domain.Entities.ProductImage
{
    ProductId = id,
    Url = publicUrl, // ✅ صار كامل
    Alt = Path.GetFileNameWithoutExtension(file.FileName),
    SortOrder = ++maxSort
};


            _db.ProductImages.Add(img);
            created.Add(new { id = img.Id, url = img.Url, alt = img.Alt, sortOrder = img.SortOrder });
        }

        await _db.SaveChangesAsync();
        return Ok(new { items = created });
    }

    // ✅ DELETE image
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage([FromRoute] Guid id, [FromRoute] Guid imageId)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.ProductId == id && x.Id == imageId);
        if (img == null) return NotFound(new { message = "Image not found" });

        // حذف الملف من wwwroot إذا موجود
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        if (img.Url.StartsWith("/"))
        {
            var local = Path.Combine(webRoot, img.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(local))
                System.IO.File.Delete(local);
        }

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    // ✅ PUT reorder images
    public record ReorderItem(Guid id, int sortOrder);

    [HttpPut("{id:guid}/images/reorder")]
    public async Task<IActionResult> Reorder([FromRoute] Guid id, [FromBody] List<ReorderItem> items)
    {
        var productExists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!productExists) return NotFound(new { message = "Product not found" });

        var map = items.ToDictionary(x => x.id, x => x.sortOrder);

        var imgs = await _db.ProductImages.Where(x => x.ProductId == id).ToListAsync();
        foreach (var img in imgs)
        {
            if (map.TryGetValue(img.Id, out var order))
                img.SortOrder = order;
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}
