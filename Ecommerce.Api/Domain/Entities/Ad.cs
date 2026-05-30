namespace Ecommerce.Api.Domain.Entities;

public enum AdType
{
    Popup = 1,
    Banner = 2,
    Product = 3,
    Slider = 4
}

public class Ad
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public AdType Type { get; set; } = AdType.Banner;

    /// <summary>
    /// مكان عرض الإعلان (مثلاً: home_top, home_middle, product_page ...)
    /// </summary>
    public string Placement { get; set; } = "home_top";

    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ImageUrlsJson { get; set; }
    public string? LinkUrl { get; set; }

    // Product Ads
    public Guid? ProductId { get; set; }

    public int SortOrder { get; set; } = 0;
    public bool IsEnabled { get; set; } = true;

    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
