using System;
using System.Collections.Generic;

namespace Ecommerce.Api.Contracts.Appearance;

public sealed class AppearanceResponse
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<string> EnabledThemes { get; set; } = new();
    public List<string> EnabledEffects { get; set; } = new();
    public List<AppearanceAdDto> Ads { get; set; } = new();
}
