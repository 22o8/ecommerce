namespace Ecommerce.Api.Domain.Entities;

public class ProductReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public int Rating { get; set; } = 5;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
