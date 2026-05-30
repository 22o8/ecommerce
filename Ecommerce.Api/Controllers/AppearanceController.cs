using System.Linq;
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
                SiteLogoUrl = null,
                IntroEnabled = false,
                IntroTitle = null,
                IntroSubtitle = null,
                IntroVideoUrl = null,
                IntroButtonText = null,
                EnabledThemes = new(),
                EnabledEffects = new(),
                Ads = new()
            });
        }

        return Ok(new AppearanceResponse
        {
            Id = config.Id,
            IsActive = config.IsActive,
            UpdatedAt = config.UpdatedAt,
            SiteLogoUrl = config.SiteLogoUrl,
            IntroEnabled = config.IntroEnabled,
            IntroTitle = config.IntroTitle,
            IntroSubtitle = config.IntroSubtitle,
            IntroVideoUrl = config.IntroVideoUrl,
            IntroButtonText = config.IntroButtonText,
            EnabledThemes = config.EnabledThemes,
            EnabledEffects = config.EnabledEffects,
            Ads = config.Ads
                .Where(a => a.IsEnabled)
                .OrderBy(a => a.SortOrder)
                .Select(a => new AppearanceAdDto(a.Id, a.Title, a.Subtitle, a.ImageUrl, a.LinkUrl, a.SortOrder, a.IsEnabled))
                .ToList()
        });
    }
}
