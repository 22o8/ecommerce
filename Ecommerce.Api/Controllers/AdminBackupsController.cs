using System.IO.Compression;
using System.Text.Json;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/backups")]
[Authorize(Roles = "Admin")]
public class AdminBackupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminBackupsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("full")]
    [RequestSizeLimit(2_000_000_000)]
    public async Task FullBackup([FromQuery] bool includeMedia = true, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var stamp = now.ToString("yyyyMMdd-HHmmss");
        var fileName = $"dr-seoul-beauty-full-backup-{stamp}.zip";

        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "application/zip";
        Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\"";
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        using var zip = new ZipArchive(Response.Body, ZipArchiveMode.Create, leaveOpen: true);

        async Task AddJsonAsync(string entryName, object data)
        {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            await using var s = entry.Open();
            await JsonSerializer.SerializeAsync(s, data, jsonOptions, ct);
        }

        var products = await _db.Products.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceIqd,
                x.DiscountPercent,
                x.PriceUsd,
                x.Brand,
                x.Category,
                x.SubCategory,
                x.ProblemCategory,
                x.ProblemSubCategory,
                x.StockQuantity,
                x.LowStockThreshold,
                x.IsCouponAllowed,
                x.IsPublished,
                x.IsFeatured,
                x.RatingAvg,
                x.RatingCount,
                x.CreatedAt,
                Images = x.Images.OrderBy(i => i.SortOrder).Select(i => new { i.Id, i.Url, i.Alt, i.SortOrder, i.CreatedAt }).ToList()
            })
            .ToListAsync(ct);

        var categories = await _db.Categories.AsNoTracking()
            .OrderBy(x => x.Section).ThenBy(x => x.ParentId).ThenBy(x => x.SortOrder).ThenBy(x => x.NameAr)
            .ToListAsync(ct);

        var brands = await _db.Brands.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var ads = await _db.Ads.AsNoTracking().OrderBy(x => x.Placement).ThenBy(x => x.SortOrder).ToListAsync(ct);
        var appearanceConfigs = await _db.AppearanceConfigs.AsNoTracking().ToListAsync(ct);
        var appearanceAds = await _db.AppearanceAds.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);
        var coupons = await _db.Coupons.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
        var reviews = await _db.ProductReviews.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        var orders = await _db.Orders.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        var orderItems = await _db.OrderItems.AsNoTracking().ToListAsync(ct);

        var backupInfo = new
        {
            store = "DR SEOUL BEAUTY",
            createdAtUtc = now,
            includeMedia,
            counts = new
            {
                products = products.Count,
                categories = categories.Count,
                brands = brands.Count,
                ads = ads.Count,
                coupons = coupons.Count,
                reviews = reviews.Count,
                orders = orders.Count,
                orderItems = orderItems.Count
            },
            note = "This ZIP contains a JSON database snapshot and, when includeMedia=true, a best-effort copy of media files downloaded from their public URLs."
        };

        await AddJsonAsync("backup-info.json", backupInfo);
        await AddJsonAsync("data/products.json", products);
        await AddJsonAsync("data/categories.json", categories);
        await AddJsonAsync("data/brands.json", brands);
        await AddJsonAsync("data/ads.json", ads);
        await AddJsonAsync("data/appearance-configs.json", appearanceConfigs);
        await AddJsonAsync("data/appearance-ads.json", appearanceAds);
        await AddJsonAsync("data/coupons.json", coupons);
        await AddJsonAsync("data/reviews.json", reviews);
        await AddJsonAsync("data/orders.json", orders);
        await AddJsonAsync("data/order-items.json", orderItems);

        var collectedMediaUrls = new List<string>();

        foreach (var product in products)
        {
            foreach (var img in product.Images)
            {
                if (IsHttpUrl(img.Url)) collectedMediaUrls.Add(img.Url!);
            }
        }

        foreach (var c in categories)
            if (IsHttpUrl(c.ImageUrl)) collectedMediaUrls.Add(c.ImageUrl!);

        foreach (var b in brands)
            if (IsHttpUrl(b.LogoUrl)) collectedMediaUrls.Add(b.LogoUrl!);

        foreach (var a in ads)
        {
            if (IsHttpUrl(a.ImageUrl)) collectedMediaUrls.Add(a.ImageUrl!);
            foreach (var url in ExtractUrlsFromJson(a.ImageUrlsJson))
                if (IsHttpUrl(url)) collectedMediaUrls.Add(url);
        }

        foreach (var a in appearanceAds)
            if (IsHttpUrl(a.ImageUrl)) collectedMediaUrls.Add(a.ImageUrl!);

        foreach (var r in reviews)
        {
            foreach (var url in ExtractUrlsFromJson(r.ImageUrlsJson))
                if (IsHttpUrl(url)) collectedMediaUrls.Add(url);
        }

        var mediaUrls = collectedMediaUrls.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        await AddJsonAsync("media/media-manifest.json", mediaUrls.Select((url, index) => new { index = index + 1, url }));

        if (!includeMedia)
            return;

        var failures = new List<object>();
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };

        for (var i = 0; i < mediaUrls.Count; i++)
        {
            if (ct.IsCancellationRequested) break;

            var url = mediaUrls[i];
            try
            {
                using var res = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                if (!res.IsSuccessStatusCode)
                {
                    failures.Add(new { url, status = (int)res.StatusCode, reason = res.ReasonPhrase });
                    continue;
                }

                var ext = GetExtensionFromUrl(url, res.Content.Headers.ContentType?.MediaType);
                var entryName = $"media/{(i + 1).ToString("D5")}-{SafeFileNameFromUrl(url)}{ext}";
                var entry = zip.CreateEntry(entryName, CompressionLevel.NoCompression);
                await using var zipStream = entry.Open();
                await using var remoteStream = await res.Content.ReadAsStreamAsync(ct);
                await remoteStream.CopyToAsync(zipStream, ct);
            }
            catch (Exception ex)
            {
                failures.Add(new { url, error = ex.Message });
            }
        }

        await AddJsonAsync("media/media-download-failures.json", failures);
    }

    private static IEnumerable<string> ExtractUrlsFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) yield break;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var s = item.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) yield return s!;
                    }
                }
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.String)
            {
                var s = doc.RootElement.GetString();
                if (!string.IsNullOrWhiteSpace(s)) yield return s!;
            }
        }
        catch
        {
            yield break;
        }
    }

    private static bool IsHttpUrl(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

    private static string SafeFileNameFromUrl(string url)
    {
        try
        {
            var u = new Uri(url);
            var name = Path.GetFileNameWithoutExtension(u.AbsolutePath);
            if (string.IsNullOrWhiteSpace(name)) name = "file";
            return string.Join("-", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        }
        catch
        {
            return "file";
        }
    }

    private static string GetExtensionFromUrl(string url, string? mediaType)
    {
        try
        {
            var ext = Path.GetExtension(new Uri(url).AbsolutePath);
            if (!string.IsNullOrWhiteSpace(ext)) return ext;
        }
        catch
        {
            // ignore
        }

        return mediaType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/avif" => ".avif",
            "video/mp4" => ".mp4",
            _ => ".bin"
        };
    }
}
