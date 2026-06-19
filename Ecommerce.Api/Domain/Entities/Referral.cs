namespace Ecommerce.Api.Domain.Entities;

public class Referral
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReferrerUserId { get; set; }
    public User? ReferrerUser { get; set; }
    public Guid ReferredUserId { get; set; }
    public User? ReferredUser { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public string Status { get; set; } = "Registered"; // Registered, Rewarded
    public bool Rewarded { get; set; } = false;
    public string? RewardType { get; set; }
    public int RewardPoints { get; set; } = 0;
    public string? RewardCouponCode { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RewardedAtUtc { get; set; }
}
