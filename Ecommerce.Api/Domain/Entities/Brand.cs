namespace Ecommerce.Api.Domain.Entities;

public class Brand
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // slug (للروابط) - نستخدمه بالفرونت
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    // لوغو مربع فقط
    public string? LogoUrl { get; set; }

    // بانر اختياري لصفحة البراند
    public string? BannerUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
