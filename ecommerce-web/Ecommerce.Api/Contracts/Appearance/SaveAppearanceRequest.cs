using System.Collections.Generic;

namespace Ecommerce.Api.Contracts.Appearance;

public sealed class SaveAppearanceRequest
{
    public List<string> EnabledThemes { get; set; } = new();
    public List<string> EnabledEffects { get; set; } = new();
    public List<AppearanceAdDto> Ads { get; set; } = new();
    public bool IsActive { get; set; } = true;
}
