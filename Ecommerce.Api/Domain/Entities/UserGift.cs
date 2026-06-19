namespace Ecommerce.Api.Domain.Entities;

public class UserGift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? ReferralId { get; set; }
    public Referral? Referral { get; set; }
    public string GiftType { get; set; } = "Points"; // Points, Coupon
    public int Points { get; set; } = 0;
    public string? CouponCode { get; set; }
    public string Title { get; set; } = "هدية جديدة";
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
