using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Ecommerce.Api.Domain.Entities;

public class AppearanceConfig
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsActive { get; set; } = true;

    // Stored as jsonb in Postgres.
    // IMPORTANT: using JsonDocument ensures Npgsql sends the correct jsonb parameter type.
    [Required]
    public JsonDocument EnabledThemesJson { get; set; } = JsonDocument.Parse("[]");

    [Required]
    public JsonDocument EnabledEffectsJson { get; set; } = JsonDocument.Parse("[]");

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<AppearanceAd> Ads { get; set; } = new();

    [NotMapped]
    public List<string> EnabledThemes
    {
        get => DeserializeList(EnabledThemesJson);
        set => EnabledThemesJson = JsonDocument.Parse(JsonSerializer.Serialize(value ?? new()));
    }

    [NotMapped]
    public List<string> EnabledEffects
    {
        get => DeserializeList(EnabledEffectsJson);
        set => EnabledEffectsJson = JsonDocument.Parse(JsonSerializer.Serialize(value ?? new()));
    }

    private static List<string> DeserializeList(JsonDocument? doc)
    {
        try
        {
            if (doc is null) return new();
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return new();
            return doc.RootElement.Deserialize<List<string>>() ?? new();
        }
        catch
        {
            return new();
        }
    }
}
