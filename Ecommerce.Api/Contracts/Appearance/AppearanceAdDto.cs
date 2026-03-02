using System;

namespace Ecommerce.Api.Contracts.Appearance;

public sealed record AppearanceAdDto(
    Guid? Id,
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? LinkUrl,
    int SortOrder,
    bool IsEnabled
);
