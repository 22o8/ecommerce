namespace Ecommerce.Api.Domain.Entities;

public class PointsTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public PointsWallet? Wallet { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public string Type { get; set; } = "Manual"; // Purchase, ReferralGift, CouponGift, Manual, Spend
    public int Points { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
