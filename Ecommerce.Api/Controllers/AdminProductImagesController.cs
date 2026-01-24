using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductImagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminProductImagesController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET /api/admin/products/{id}/images
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

    // POST /api/admin/products/{id}/images
    // Swagger expects multipart field: images[]
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(30_000_000)] // 30MB
    public async Task<IActionResult> UploadImages(
        [FromRoute] Guid id,
        [FromForm(Name = "images")] List<IFormFile> images,
        [FromForm] string? alt
    )
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound(new { message = "Product not found" });

        if (images == null || images.Count == 0)
            return BadRequest(new { message = "No images uploaded." });

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

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

        foreach (var file in images)
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
                Url = publicUrl,
                Alt = string.IsNullOrWhiteSpace(alt)
                    ? Path.GetFileNameWithoutExtension(file.FileName)
                    : alt.Trim(),
                SortOrder = ++maxSort
            };

            _db.ProductImages.Add(img);
            created.Add(new { id = img.Id, url = img.Url, alt = img.Alt, sortOrder = img.SortOrder });
        }

        await _db.SaveChangesAsync();
        return Ok(new { items = created });
    }

    // DELETE /api/admin/products/{id}/images/{imageId}
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage([FromRoute] Guid id, [FromRoute] Guid imageId)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.ProductId == id && x.Id == imageId);
        if (img == null) return NotFound(new { message = "Image not found" });

        // حذف الملف من wwwroot إذا كان محلي
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        if (!string.IsNullOrWhiteSpace(img.Url))
        {
            try
            {
                var u = new Uri(img.Url, UriKind.RelativeOrAbsolute);
                var path = u.IsAbsoluteUri ? u.AbsolutePath : img.Url;
                if (path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    var local = Path.Combine(webRoot, path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(local))
                        System.IO.File.Delete(local);
                }
            }
            catch { /* ignore */ }
        }

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    public record ReorderImagesRequest(List<Guid> ImageIds);

    // PUT /api/admin/products/{id}/images/reorder  body: { imageIds: [uuid...] }
    [HttpPut("{id:guid}/images/reorder")]
    public async Task<IActionResult> Reorder([FromRoute] Guid id, [FromBody] ReorderImagesRequest req)
    {
        var productExists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!productExists) return NotFound(new { message = "Product not found" });

        var ids = req.ImageIds ?? new List<Guid>();
        var imgs = await _db.ProductImages.Where(x => x.ProductId == id).ToListAsync();

        // set new order
        var orderMap = ids.Select((imgId, idx) => new { imgId, idx }).ToDictionary(x => x.imgId, x => x.idx);

        foreach (var img in imgs)
        {
            if (orderMap.TryGetValue(img.Id, out var order))
                img.SortOrder = order;
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}
