using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Api.Contracts.Appearance;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/appearance")]
public class AppearanceController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppearanceController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<AppearanceResponse>> Get()
    {
        var config = await _db.AppearanceConfigs
            .Include(x => x.Ads)
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync();

        if (config is null)
        {
            return Ok(new AppearanceResponse
            {
                Id = System.Guid.Empty,
                IsActive = true,
                UpdatedAt = System.DateTimeOffset.UtcNow,
                EnabledThemes = new(),
                EnabledEffects = new(),
                Ads = new()
            });
        }

        var themes = SafeParseList(config.EnabledThemesJson);
        var effects = SafeParseList(config.EnabledEffectsJson);

        return Ok(new AppearanceResponse
        {
            Id = config.Id,
            IsActive = config.IsActive,
            UpdatedAt = config.UpdatedAt,
            EnabledThemes = themes,
            EnabledEffects = effects,
            Ads = config.Ads
                .Where(a => a.IsEnabled)
                .OrderBy(a => a.SortOrder)
                .Select(a => new AppearanceAdDto(a.Id, a.Title, a.Subtitle, a.ImageUrl, a.LinkUrl, a.SortOrder, a.IsEnabled))
                .ToList()
        });
    }

    private static System.Collections.Generic.List<string> SafeParseList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }
}
