namespace Ecommerce.Api.Domain.Entities;

public class SiteVisit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string? Path { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
