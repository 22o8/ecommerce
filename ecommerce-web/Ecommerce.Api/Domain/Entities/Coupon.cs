namespace Ecommerce.Api.Domain.Entities;

public class Coupon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int DiscountPercent { get; set; } = 0;
    public decimal FixedDiscountIqd { get; set; } = 0m;
    public decimal MinimumOrderIqd { get; set; } = 0m;
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime? StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
