using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Ecommerce.Api.Domain.Entities;

public class AppearanceConfig
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsActive { get; set; } = true;

    // Stored as jsonb for portability.
    [Required]
    public string EnabledThemesJson { get; set; } = "[]";

    [Required]
    public string EnabledEffectsJson { get; set; } = "[]";

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<AppearanceAd> Ads { get; set; } = new();

    [NotMapped]
    public List<string> EnabledThemes
    {
        get => DeserializeList(EnabledThemesJson);
        set => EnabledThemesJson = JsonSerializer.Serialize(value ?? new());
    }

    [NotMapped]
    public List<string> EnabledEffects
    {
        get => DeserializeList(EnabledEffectsJson);
        set => EnabledEffectsJson = JsonSerializer.Serialize(value ?? new());
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }
}
