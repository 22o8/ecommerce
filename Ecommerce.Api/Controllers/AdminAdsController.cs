using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/ads")]
[Authorize(Roles = "Admin")]
public class AdminAdsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public AdminAdsController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? placement = null, [FromQuery] AdType? type = null)
    {
        var q = _db.Ads.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(placement)) q = q.Where(x => x.Placement == placement);
        if (type.HasValue) q = q.Where(x => x.Type == type.Value);

        var items = await q
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Placement)
            .ThenBy(x => x.SortOrder)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.Id,
                type = x.Type.ToString().ToLowerInvariant(),
                x.Placement,
                x.Title,
                x.Subtitle,
                x.ImageUrl,
                x.LinkUrl,
                x.ProductId,
                x.SortOrder,
                x.IsEnabled,
                x.StartAt,
                x.EndAt,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var x = await _db.Ads.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (x is null) return NotFound();

        return Ok(new
        {
            x.Id,
            type = x.Type.ToString().ToLowerInvariant(),
            x.Placement,
            x.Title,
            x.Subtitle,
            x.ImageUrl,
            x.LinkUrl,
            x.ProductId,
            x.SortOrder,
            x.IsEnabled,
            x.StartAt,
            x.EndAt,
            x.CreatedAt,
            x.UpdatedAt
        });
    }

    public record SaveAdRequest(
        string Type,
        string Placement,
        string Title,
        string? Subtitle,
        string ImageUrl,
        string? LinkUrl,
        Guid? ProductId,
        int SortOrder,
        bool IsEnabled,
        DateTimeOffset? StartAt,
        DateTimeOffset? EndAt
    );

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveAdRequest req)
    {
        var type = ParseType(req.Type);
        var ad = new Ad
        {
            Type = type,
            Placement = string.IsNullOrWhiteSpace(req.Placement) ? "home_top" : req.Placement.Trim(),
            Title = req.Title ?? string.Empty,
            Subtitle = req.Subtitle,
            ImageUrl = req.ImageUrl ?? string.Empty,
            LinkUrl = req.LinkUrl,
            ProductId = req.ProductId,
            SortOrder = req.SortOrder,
            IsEnabled = req.IsEnabled,
            StartAt = req.StartAt,
            EndAt = req.EndAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Ads.Add(ad);
        await _db.SaveChangesAsync();
        return Ok(new { ad.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveAdRequest req)
    {
        var ad = await _db.Ads.FirstOrDefaultAsync(x => x.Id == id);
        if (ad is null) return NotFound();

        ad.Type = ParseType(req.Type);
        ad.Placement = string.IsNullOrWhiteSpace(req.Placement) ? ad.Placement : req.Placement.Trim();
        ad.Title = req.Title ?? string.Empty;
        ad.Subtitle = req.Subtitle;
        ad.ImageUrl = req.ImageUrl ?? string.Empty;
        ad.LinkUrl = req.LinkUrl;
        ad.ProductId = req.ProductId;
        ad.SortOrder = req.SortOrder;
        ad.IsEnabled = req.IsEnabled;
        ad.StartAt = req.StartAt;
        ad.EndAt = req.EndAt;
        ad.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { ad.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ad = await _db.Ads.FirstOrDefaultAsync(x => x.Id == id);
        if (ad is null) return NotFound();
        _db.Ads.Remove(ad);
        await _db.SaveChangesAsync();
        return Ok();
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
        var key = $"uploads/ads/{id}{ext}";

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, key, file.ContentType);

        return Ok(new { url });
    }

    private static AdType ParseType(string? t)
    {
        var s = (t ?? "").Trim().ToLowerInvariant();
        return s switch
        {
            "popup" => AdType.Popup,
            "banner" => AdType.Banner,
            "product" or "productads" or "product_ad" => AdType.Product,
            _ => AdType.Banner
        };
    }
}
