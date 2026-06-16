namespace Ecommerce.Api.Domain.Entities;

public class AdminActivity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? AdminUserId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
