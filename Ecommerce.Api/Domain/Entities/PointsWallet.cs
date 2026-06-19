namespace Ecommerce.Api.Domain.Entities;

public class PointsWallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public int Balance { get; set; } = 0;
    public int LifetimeEarned { get; set; } = 0;
    public int LifetimeSpent { get; set; } = 0;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<PointsTransaction> Transactions { get; set; } = new();
}
