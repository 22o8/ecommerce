using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Images;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/media-optimization")]
[Route("api/bff/admin/media-optimization")]
public sealed class AdminMediaOptimizationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MediaOptimizationService _media;

    public AdminMediaOptimizationController(AppDbContext db, MediaOptimizationService media)
    {
        _db = db;
        _media = media;
    }

    [HttpGet("audit")]
    public async Task<IActionResult> Audit(CancellationToken ct)
    {
        var productImages = await _db.ProductImages.CountAsync(x => !string.IsNullOrWhiteSpace(x.Url) && !x.Url.Contains("/optimized/"), ct);
        var brands = await _db.Brands.CountAsync(x => !string.IsNullOrWhiteSpace(x.LogoUrl) && !x.LogoUrl!.Contains("/optimized/"), ct);
        var categories = await _db.Categories.CountAsync(x => !string.IsNullOrWhiteSpace(x.ImageUrl) && !x.ImageUrl!.Contains("/optimized/"), ct);
        var ads = await _db.Ads.CountAsync(x => !string.IsNullOrWhiteSpace(x.ImageUrl) && !x.ImageUrl.Contains("/optimized/") && !IsVideoUrl(x.ImageUrl), ct);
        var appearanceAds = await _db.AppearanceAds.CountAsync(x => !string.IsNullOrWhiteSpace(x.ImageUrl) && !x.ImageUrl!.Contains("/optimized/"), ct);
        var appearanceLogos = await _db.AppearanceConfigs.CountAsync(x => !string.IsNullOrWhiteSpace(x.SiteLogoUrl) && !x.SiteLogoUrl!.Contains("/optimized/"), ct);

        return Ok(new
        {
            pending = productImages + brands + categories + ads + appearanceAds + appearanceLogos,
            productImages,
            brands,
            categories,
            ads,
            appearanceAds,
            appearanceLogos,
            note = "استعمل POST migrate?apply=true&limit=20 لتحويل الملفات القديمة تدريجياً."
        });
    }

    [HttpPost("migrate")]
    public async Task<IActionResult> Migrate([FromQuery] bool apply = false, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        var done = 0;
        var skipped = 0;
        var failed = new List<object>();

        async Task<bool> TryOptimize(string? oldUrl, string keyPrefix, Action<string> setUrl, string label)
        {
            if (done >= limit) return false;
            if (string.IsNullOrWhiteSpace(oldUrl) || oldUrl.Contains("/optimized/", StringComparison.OrdinalIgnoreCase) || IsVideoUrl(oldUrl))
            {
                skipped++;
                return false;
            }

            try
            {
                var optimized = await _media.OptimizeRemoteImageAsync(oldUrl, keyPrefix, ct);
                if (optimized is null)
                {
                    skipped++;
                    return false;
                }

                if (apply) setUrl(optimized.Url);
                done++;
                return true;
            }
            catch (Exception ex)
            {
                failed.Add(new { label, oldUrl, error = ex.Message });
                return false;
            }
        }

        var productImages = await _db.ProductImages
            .Where(x => !string.IsNullOrWhiteSpace(x.Url) && !x.Url.Contains("/optimized/"))
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        foreach (var img in productImages)
            await TryOptimize(img.Url, $"products/{img.ProductId}", url => img.Url = url, $"ProductImage:{img.Id}");

        if (done < limit)
        {
            var categories = await _db.Categories
                .Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl) && !x.ImageUrl!.Contains("/optimized/"))
                .OrderBy(x => x.UpdatedAt)
                .Take(limit - done)
                .ToListAsync(ct);
            foreach (var c in categories)
                await TryOptimize(c.ImageUrl, "categories", url => { c.ImageUrl = url; c.UpdatedAt = DateTime.UtcNow; }, $"Category:{c.Id}");
        }

        if (done < limit)
        {
            var brands = await _db.Brands
                .Where(x => !string.IsNullOrWhiteSpace(x.LogoUrl) && !x.LogoUrl!.Contains("/optimized/"))
                .OrderBy(x => x.CreatedAt)
                .Take(limit - done)
                .ToListAsync(ct);
            foreach (var b in brands)
                await TryOptimize(b.LogoUrl, "brands", url => b.LogoUrl = url, $"Brand:{b.Id}");
        }

        if (done < limit)
        {
            var ads = await _db.Ads
                .Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl) && !x.ImageUrl.Contains("/optimized/"))
                .OrderBy(x => x.CreatedAt)
                .Take(limit - done)
                .ToListAsync(ct);
            foreach (var ad in ads)
                await TryOptimize(ad.ImageUrl, "ads", url => { ad.ImageUrl = url; ad.UpdatedAt = DateTimeOffset.UtcNow; }, $"Ad:{ad.Id}");
        }

        if (done < limit)
        {
            var appearanceAds = await _db.AppearanceAds
                .Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl) && !x.ImageUrl!.Contains("/optimized/"))
                .OrderBy(x => x.CreatedAt)
                .Take(limit - done)
                .ToListAsync(ct);
            foreach (var ad in appearanceAds)
                await TryOptimize(ad.ImageUrl, "appearance-ads", url => ad.ImageUrl = url, $"AppearanceAd:{ad.Id}");
        }

        if (done < limit)
        {
            var configs = await _db.AppearanceConfigs
                .Where(x => !string.IsNullOrWhiteSpace(x.SiteLogoUrl) && !x.SiteLogoUrl!.Contains("/optimized/"))
                .OrderByDescending(x => x.UpdatedAt)
                .Take(limit - done)
                .ToListAsync(ct);
            foreach (var cfg in configs)
                await TryOptimize(cfg.SiteLogoUrl, "appearance", url => { cfg.SiteLogoUrl = url; cfg.UpdatedAt = DateTimeOffset.UtcNow; }, $"AppearanceConfig:{cfg.Id}");
        }

        if (apply && done > 0)
            await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            apply,
            optimized = done,
            skipped,
            failedCount = failed.Count,
            failed,
            next = done == limit ? "شغل نفس الرابط مرة ثانية حتى يكمل الباقي تدريجياً." : "لا توجد عناصر أكثر ضمن هذه الدفعة."
        });
    }

    private static bool IsVideoUrl(string url)
    {
        return url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
            || url.EndsWith(".webm", StringComparison.OrdinalIgnoreCase)
            || url.EndsWith(".mov", StringComparison.OrdinalIgnoreCase);
    }
}
