using System.Text.RegularExpressions;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Images;
using Ecommerce.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/packages")]
[Authorize(Roles = "Admin")]
public class AdminPackagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public AdminPackagesController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.ProductPackages
            .AsNoTracking()
            .Include(x => x.Items).ThenInclude(i => i.Product)
            .OrderBy(x => x.SortOrder).ThenByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var item = await _db.ProductPackages.AsNoTracking().Include(x => x.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(x => x.Id == id);
        return item == null ? NotFound() : Ok(ToDto(item));
    }

    [HttpGet("product-search")]
    public async Task<IActionResult> ProductSearch([FromQuery] string? q = null)
    {
        var query = _db.Products.AsNoTracking().Include(p => p.Images).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(term) || p.Brand.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }
        var products = await query.OrderByDescending(p => p.CreatedAt).Take(30).Select(p => new
        {
            p.Id,
            p.Title,
            p.Slug,
            p.Brand,
            p.PriceIqd,
            p.StockQuantity,
            p.IsPublished,
            image = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
        }).ToListAsync();
        return Ok(products);
    }

    public sealed class SavePackageRequest
    {
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? SeoDescription { get; set; }
        public string? Note { get; set; }
        public string? CoverUrl { get; set; }
        public string? MediaType { get; set; }
        public decimal FinalPriceIqd { get; set; }
        public decimal? OriginalPriceIqd { get; set; }
        public string? Category { get; set; }
        public string? ProblemCategory { get; set; }
        public bool IsPublished { get; set; } = true;
        public bool IsFeatured { get; set; }
        public bool ShowInSlider { get; set; }
        public string? SliderPlacement { get; set; }
        public int SortOrder { get; set; }
        public List<PackageItemRequest> Items { get; set; } = new();
    }

    public sealed class PackageItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SavePackageRequest req)
    {
        var validation = await ValidateRequest(req);
        if (validation.Error != null) return validation.Error;
        var slug = await BuildUniqueSlug(req.Slug, req.NameEn, req.NameAr, null);
        var package = new ProductPackage
        {
            Id = Guid.NewGuid(),
            NameAr = (req.NameAr ?? "").Trim(),
            NameEn = (req.NameEn ?? "").Trim(),
            Slug = slug,
            ShortDescription = (req.ShortDescription ?? "").Trim(),
            SeoDescription = (req.SeoDescription ?? "").Trim(),
            Note = (req.Note ?? "").Trim(),
            CoverUrl = (req.CoverUrl ?? "").Trim(),
            MediaType = string.IsNullOrWhiteSpace(req.MediaType) ? "image" : req.MediaType.Trim(),
            OriginalPriceIqd = req.OriginalPriceIqd.GetValueOrDefault(validation.OriginalPrice),
            FinalPriceIqd = Math.Max(0, req.FinalPriceIqd),
            Category = (req.Category ?? "").Trim(),
            ProblemCategory = (req.ProblemCategory ?? "").Trim(),
            IsPublished = req.IsPublished,
            IsFeatured = req.IsFeatured,
            ShowInSlider = req.ShowInSlider,
            SliderPlacement = string.IsNullOrWhiteSpace(req.SliderPlacement) ? "packages" : req.SliderPlacement.Trim(),
            SortOrder = req.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = req.Items.Select((x, idx) => new ProductPackageItem { ProductId = x.ProductId, Quantity = Math.Max(1, x.Quantity), SortOrder = idx }).ToList()
        };
        _db.ProductPackages.Add(package);
        await _db.SaveChangesAsync();
        return Ok(ToDto(await _db.ProductPackages.Include(x => x.Items).ThenInclude(i => i.Product).FirstAsync(x => x.Id == package.Id)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SavePackageRequest req)
    {
        var package = await _db.ProductPackages.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (package == null) return NotFound();
        var validation = await ValidateRequest(req);
        if (validation.Error != null) return validation.Error;
        package.NameAr = (req.NameAr ?? "").Trim();
        package.NameEn = (req.NameEn ?? "").Trim();
        package.Slug = await BuildUniqueSlug(req.Slug, req.NameEn, req.NameAr, id);
        package.ShortDescription = (req.ShortDescription ?? "").Trim();
        package.SeoDescription = (req.SeoDescription ?? "").Trim();
        package.Note = (req.Note ?? "").Trim();
        package.CoverUrl = (req.CoverUrl ?? "").Trim();
        package.MediaType = string.IsNullOrWhiteSpace(req.MediaType) ? "image" : req.MediaType.Trim();
        package.OriginalPriceIqd = req.OriginalPriceIqd.GetValueOrDefault(validation.OriginalPrice);
        package.FinalPriceIqd = Math.Max(0, req.FinalPriceIqd);
        package.Category = (req.Category ?? "").Trim();
        package.ProblemCategory = (req.ProblemCategory ?? "").Trim();
        package.IsPublished = req.IsPublished;
        package.IsFeatured = req.IsFeatured;
        package.ShowInSlider = req.ShowInSlider;
        package.SliderPlacement = string.IsNullOrWhiteSpace(req.SliderPlacement) ? "packages" : req.SliderPlacement.Trim();
        package.SortOrder = req.SortOrder;
        package.UpdatedAt = DateTime.UtcNow;
        _db.ProductPackageItems.RemoveRange(package.Items);
        await _db.SaveChangesAsync();
        package.Items = req.Items.Select((x, idx) => new ProductPackageItem { ProductPackageId = package.Id, ProductId = x.ProductId, Quantity = Math.Max(1, x.Quantity), SortOrder = idx }).ToList();
        await _db.SaveChangesAsync();
        return Ok(ToDto(await _db.ProductPackages.Include(x => x.Items).ThenInclude(i => i.Product).FirstAsync(x => x.Id == package.Id)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var package = await _db.ProductPackages.FirstOrDefaultAsync(x => x.Id == id);
        if (package == null) return NotFound();
        _db.ProductPackages.Remove(package);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Package deleted" });
    }

    [HttpPost("upload")]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file)
    {
        if ((file == null || file.Length == 0) && Request.HasFormContentType && Request.Form.Files.Count > 0) file = Request.Form.Files[0];
        if (file == null || file.Length == 0) return BadRequest(new { message = "File is required" });
        var id = Guid.NewGuid();
        if ((file.ContentType ?? "").StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".mp4";
            var key = $"uploads/packages/{id}{ext}";
            await using var stream = file.OpenReadStream();
            var stored = await _storage.UploadAsync(stream, key, file.ContentType, HttpContext.RequestAborted);
            return Ok(new { url = stored.Url, key = stored.Key, mediaType = "video" });
        }
        var optimized = await ImageOptimizer.OptimizeImageToWebpAsync(file, HttpContext.RequestAborted);
        await using var img = optimized.Stream;
        var imgKey = $"uploads/packages/{id}{optimized.Extension}";
        var saved = await _storage.UploadAsync(img, imgKey, optimized.ContentType, HttpContext.RequestAborted);
        return Ok(new { url = saved.Url, key = saved.Key, mediaType = "image", optimized = optimized.Optimized });
    }

    private async Task<(IActionResult? Error, decimal OriginalPrice)> ValidateRequest(SavePackageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NameAr) && string.IsNullOrWhiteSpace(req.NameEn)) return (BadRequest(new { message = "Package name is required" }), 0m);
        if (req.Items.Count == 0) return (BadRequest(new { message = "Select at least one product" }), 0m);
        var ids = req.Items.Select(x => x.ProductId).Distinct().ToList();
        if (ids.Count != req.Items.Count) return (BadRequest(new { message = "Duplicate product inside package" }), 0m);
        var products = await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
        if (products.Count != ids.Count) return (BadRequest(new { message = "One or more selected products were not found" }), 0m);
        var original = req.Items.Sum(i => (products.First(p => p.Id == i.ProductId).PriceIqd) * Math.Max(1, i.Quantity));
        if (req.FinalPriceIqd <= 0) return (BadRequest(new { message = "Final price is required" }), original);
        return (null, original);
    }

    private async Task<string> BuildUniqueSlug(string? requested, string? nameEn, string? nameAr, Guid? currentId)
    {
        var baseSlug = Slugify(requested ?? nameEn ?? nameAr ?? Guid.NewGuid().ToString("N"));
        if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = $"package-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var slug = baseSlug;
        var n = 2;
        while (await _db.ProductPackages.AnyAsync(x => x.Slug == slug && (!currentId.HasValue || x.Id != currentId.Value))) slug = $"{baseSlug}-{n++}";
        return slug;
    }

    private static string Slugify(string value)
    {
        var s = (value ?? "").Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^\p{L}\p{Nd}]+", "-");
        s = Regex.Replace(s, "-+", "-").Trim('-');
        return s.Length > 160 ? s[..160].Trim('-') : s;
    }

    internal static object ToDto(ProductPackage x)
    {
        var warnings = x.Items.Where(i => i.Product == null || !i.Product.IsPublished || i.Product.StockQuantity < i.Quantity)
            .Select(i => new { productId = i.ProductId, productTitle = i.Product?.Title ?? "منتج غير موجود", required = i.Quantity, available = i.Product?.StockQuantity ?? 0, reason = i.Product == null ? "missing" : (!i.Product.IsPublished ? "unpublished" : "low_stock") })
            .ToList();
        var maxAvailable = x.Items.Count == 0 ? 0 : x.Items.Select(i => i.Product == null ? 0 : (i.Product.StockQuantity / Math.Max(1, i.Quantity))).DefaultIfEmpty(0).Min();
        return new
        {
            x.Id,
            x.NameAr,
            x.NameEn,
            name = string.IsNullOrWhiteSpace(x.NameAr) ? x.NameEn : x.NameAr,
            x.Slug,
            x.ShortDescription,
            x.SeoDescription,
            x.Note,
            x.CoverUrl,
            x.MediaType,
            x.OriginalPriceIqd,
            x.FinalPriceIqd,
            savingsIqd = Math.Max(0, x.OriginalPriceIqd - x.FinalPriceIqd),
            savingsPercent = x.OriginalPriceIqd > 0 ? (int)Math.Round((x.OriginalPriceIqd - x.FinalPriceIqd) * 100m / x.OriginalPriceIqd) : 0,
            x.Category,
            x.ProblemCategory,
            x.IsPublished,
            x.IsFeatured,
            x.ShowInSlider,
            x.SliderPlacement,
            x.SortOrder,
            x.SoldCount,
            availablePackages = maxAvailable,
            canSell = x.IsPublished && warnings.Count == 0 && maxAvailable > 0,
            warnings,
            items = x.Items.OrderBy(i => i.SortOrder).Select(i => new
            {
                i.Id,
                i.ProductId,
                i.Quantity,
                productTitle = i.Product?.Title,
                productSlug = i.Product?.Slug,
                brand = i.Product?.Brand,
                priceIqd = i.Product?.PriceIqd ?? 0,
                stockQuantity = i.Product?.StockQuantity ?? 0,
                isPublished = i.Product?.IsPublished ?? false
            }).ToList(),
            x.CreatedAt,
            x.UpdatedAt
        };
    }
}
