using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Domain.Entities;

public class AppearanceAd
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AppearanceConfigId { get; set; }

    public AppearanceConfig? AppearanceConfig { get; set; }

    [MaxLength(120)]
    public string? Title { get; set; }

    [MaxLength(200)]
    public string? Subtitle { get; set; }

    [Required]
    [MaxLength(2048)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string? LinkUrl { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsEnabled { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
