using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Api.Contracts.Appearance;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Storage;
using Ecommerce.Api.Infrastructure.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/appearance")]
[Authorize(Roles = "Admin")]
public class AdminAppearanceController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public AdminAppearanceController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<ActionResult<AppearanceResponse>> Get()
    {
        var config = await GetOrCreateConfig();
        return Ok(ToResponse(config));
    }

    [HttpPost]
    public async Task<ActionResult<AppearanceResponse>> Save([FromBody] SaveAppearanceRequest req)
    {
        var config = await GetOrCreateConfig();

        config.IsActive = req.IsActive;
        // Persist as jsonb via JsonDocument-backed properties
        config.EnabledThemes = req.EnabledThemes ?? new();
        config.EnabledEffects = req.EnabledEffects ?? new();
        config.SiteLogoUrl = string.IsNullOrWhiteSpace(req.SiteLogoUrl) ? null : req.SiteLogoUrl.Trim();
        config.IntroEnabled = req.IntroEnabled;
        config.IntroVideoUrl = string.IsNullOrWhiteSpace(req.IntroVideoUrl) ? null : req.IntroVideoUrl.Trim();
        config.IntroTitle = req.IntroTitle ?? string.Empty;
        config.IntroSubtitle = req.IntroSubtitle;
        config.IntroButtonText = string.IsNullOrWhiteSpace(req.IntroButtonText) ? "ابدأ الآن" : req.IntroButtonText.Trim();
        config.IntroButtonUrl = string.IsNullOrWhiteSpace(req.IntroButtonUrl) ? "/products" : req.IntroButtonUrl.Trim();
        config.IntroSecondaryButtonText = string.IsNullOrWhiteSpace(req.IntroSecondaryButtonText) ? "تصفح البراندات" : req.IntroSecondaryButtonText.Trim();
        config.IntroSecondaryButtonUrl = string.IsNullOrWhiteSpace(req.IntroSecondaryButtonUrl) ? "/brands" : req.IntroSecondaryButtonUrl.Trim();
        config.UpdatedAt = DateTimeOffset.UtcNow;

        // Sync ads
        var incoming = (req.Ads ?? new()).Select(a => new
        {
            Id = a.Id,
            a.Title,
            a.Subtitle,
            a.ImageUrl,
            a.LinkUrl,
            a.SortOrder,
            a.IsEnabled
        }).ToList();

        var keepIds = incoming.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();

        // remove deleted
        config.Ads.RemoveAll(a => !keepIds.Contains(a.Id));

        // update existing
        foreach (var item in incoming.Where(x => x.Id.HasValue))
        {
            var ad = config.Ads.FirstOrDefault(x => x.Id == item.Id!.Value);
            if (ad is null) continue;
            ad.Title = item.Title ?? string.Empty;
            ad.Subtitle = item.Subtitle;
            ad.ImageUrl = item.ImageUrl ?? string.Empty;
            ad.LinkUrl = item.LinkUrl;
            ad.SortOrder = item.SortOrder;
            ad.IsEnabled = item.IsEnabled;
        }

        // add new
        foreach (var item in incoming.Where(x => !x.Id.HasValue))
        {
            if (string.IsNullOrWhiteSpace(item.ImageUrl))
                continue;

            config.Ads.Add(new AppearanceAd
            {
                Id = Guid.NewGuid(),
                AppearanceConfigId = config.Id,
                Title = item.Title ?? string.Empty,
                Subtitle = item.Subtitle,
                ImageUrl = item.ImageUrl,
                LinkUrl = item.LinkUrl,
                SortOrder = item.SortOrder,
                IsEnabled = item.IsEnabled
            });
        }

        await _db.SaveChangesAsync();
        return Ok(ToResponse(config));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    public async Task<ActionResult<object>> Upload([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        var isVideo = (file.ContentType ?? string.Empty).StartsWith("video/", StringComparison.OrdinalIgnoreCase);
        var id = Guid.NewGuid();

        if (isVideo)
        {
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".mp4";
            var key = $"uploads/intro-videos/{id}{ext}";
            await using var videoStream = file.OpenReadStream();
            var storedVideo = await _storage.UploadAsync(videoStream, key, string.IsNullOrWhiteSpace(file.ContentType) ? "video/mp4" : file.ContentType, HttpContext.RequestAborted);
            return Ok(new { url = storedVideo.Url, key = storedVideo.Key, optimized = false });
        }

        var optimized = await ImageOptimizer.OptimizeImageToWebpAsync(file, HttpContext.RequestAborted);
        await using var stream = optimized.Stream;
        var imageKey = $"uploads/appearance/{id}{optimized.Extension}";
        var stored = await _storage.UploadAsync(stream, imageKey, optimized.ContentType, HttpContext.RequestAborted);

        return Ok(new { url = stored.Url, key = stored.Key, optimized = optimized.Optimized });
    }

    private async Task<AppearanceConfig> GetOrCreateConfig()
    {
        var config = await _db.AppearanceConfigs
            .Include(x => x.Ads)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync();

        if (config is not null) return config;

        config = new AppearanceConfig
        {
            EnabledThemesJson = JsonDocument.Parse("[]"),
            EnabledEffectsJson = JsonDocument.Parse("[]"),
            IsActive = true,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.AppearanceConfigs.Add(config);
        await _db.SaveChangesAsync();
        return config;
    }

    private static AppearanceResponse ToResponse(AppearanceConfig config)
    {
        return new AppearanceResponse
        {
            Id = config.Id,
            IsActive = config.IsActive,
            UpdatedAt = config.UpdatedAt,
            EnabledThemes = config.EnabledThemes,
            EnabledEffects = config.EnabledEffects,
            Ads = config.Ads
                .OrderBy(a => a.SortOrder)
                .Select(a => new AppearanceAdDto(a.Id, a.Title, a.Subtitle, a.ImageUrl, a.LinkUrl, a.SortOrder, a.IsEnabled))
                .ToList(),
            SiteLogoUrl = config.SiteLogoUrl,
            IntroEnabled = config.IntroEnabled,
            IntroVideoUrl = config.IntroVideoUrl,
            IntroTitle = config.IntroTitle,
            IntroSubtitle = config.IntroSubtitle,
            IntroButtonText = config.IntroButtonText,
            IntroButtonUrl = config.IntroButtonUrl,
            IntroSecondaryButtonText = config.IntroSecondaryButtonText,
            IntroSecondaryButtonUrl = config.IntroSecondaryButtonUrl
        };
    }
}
