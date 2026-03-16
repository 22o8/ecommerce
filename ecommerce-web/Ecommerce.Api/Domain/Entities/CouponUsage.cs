namespace Ecommerce.Api.Domain.Entities;

public class CouponUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public string DeviceKeyHash { get; set; } = string.Empty;
    public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;
}
