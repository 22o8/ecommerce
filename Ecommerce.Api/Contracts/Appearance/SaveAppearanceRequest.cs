using System.Collections.Generic;

namespace Ecommerce.Api.Contracts.Appearance;

public sealed class SaveAppearanceRequest
{
    public List<string> EnabledThemes { get; set; } = new();
    public List<string> EnabledEffects { get; set; } = new();
    public List<AppearanceAdDto> Ads { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public string? SiteLogoUrl { get; set; }
    public bool IntroEnabled { get; set; }
    public string? IntroVideoUrl { get; set; }
    public string? IntroTitle { get; set; }
    public string? IntroSubtitle { get; set; }
    public string? IntroButtonText { get; set; }
    public string? IntroButtonUrl { get; set; }
    public string? IntroSecondaryButtonText { get; set; }
    public string? IntroSecondaryButtonUrl { get; set; }
}
