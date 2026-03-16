// Ecommerce.Api/Domain/Entities/Brand.cs
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Domain.Entities;

public class Brand
{
    public Guid Id { get; set; }

    // slug used in URLs and as Product.Brand value
    [Required, MaxLength(80)]
    public string Slug { get; set; } = "";

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(400)]
    public string? Description { get; set; }

    // Square logo URL (served from /uploads/brands/...)
    [MaxLength(400)]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} // ✅ لازم هذا القوس
