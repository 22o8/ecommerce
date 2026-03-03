using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Api.Contracts.Appearance;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Storage;
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
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<object>> Upload([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var id = Guid.NewGuid();
        var key = $"uploads/appearance/{id}{ext}";

        await using var stream = file.OpenReadStream();
        // IStorageService.UploadAsync expects (Stream stream, string key, string contentType)
        var url = await _storage.UploadAsync(stream, key, file.ContentType);

        return Ok(new { url });
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
                .ToList()
        };
    }
}
