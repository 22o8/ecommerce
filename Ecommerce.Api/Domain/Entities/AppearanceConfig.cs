using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Ecommerce.Api.Domain.Entities;

public class AppearanceConfig
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsActive { get; set; } = true;

    // نخزنها كنص (TEXT) يحتوي JSON.
    // سابقاً كانت jsonb لكن EF/Npgsql كان يرسلها كـ text بدون cast فتصير 500.
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
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            var list = JsonSerializer.Deserialize<List<string>>(json) ?? new();
            return list
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return new();
        }
    }
}
