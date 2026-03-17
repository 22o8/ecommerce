using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/ads/active?placement=home_top&type=banner&productId=...
    [HttpGet("active")]
    public async Task<IActionResult> Active([FromQuery] string? placement = null, [FromQuery] string? type = null, [FromQuery] Guid? productId = null)
    {
        var now = DateTimeOffset.UtcNow;
        var q = _db.Ads.AsNoTracking().Where(x => x.IsEnabled);

        if (!string.IsNullOrWhiteSpace(placement))
            q = q.Where(x => x.Placement == placement);

        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = (type ?? "").Trim().ToLowerInvariant();
            q = t switch
            {
                "popup" => q.Where(x => x.Type == AdType.Popup),
                "banner" => q.Where(x => x.Type == AdType.Banner),
                "product" or "productads" or "product_ad" => q.Where(x => x.Type == AdType.Product),
                "slider" or "carousel" => q.Where(x => x.Type == AdType.Slider),
                _ => q
            };
        }

        // time window
        q = q.Where(x => (x.StartAt == null || x.StartAt <= now) && (x.EndAt == null || x.EndAt >= now));

        // Product Ads (اختياري)
        if (productId.HasValue)
            q = q.Where(x => x.ProductId == null || x.ProductId == productId);

        var items = await q
            .OrderBy(x => x.SortOrder)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.Id,
                type = x.Type.ToString().ToLowerInvariant(),
                x.Placement,
                x.Title,
                x.Subtitle,
                x.ImageUrl,
                imageUrls = ParseImageUrls(x.ImageUrlsJson, x.ImageUrl),
                x.LinkUrl,
                x.ProductId,
                x.SortOrder,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    private static List<string> ParseImageUrls(string? json, string? fallbackImage)
    {
        try
        {
            var arr = string.IsNullOrWhiteSpace(json)
                ? new List<string>()
                : (JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>());
            if (arr.Count == 0 && !string.IsNullOrWhiteSpace(fallbackImage)) arr.Add(fallbackImage);
            return arr.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
        catch
        {
            return string.IsNullOrWhiteSpace(fallbackImage) ? new List<string>() : new List<string> { fallbackImage! };
        }
    }
}
