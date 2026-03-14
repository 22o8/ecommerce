namespace Ecommerce.Api.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public decimal TotalUsd { get; set; }
    public decimal TotalIqd { get; set; }
    public decimal SubtotalUsd { get; set; }
    public decimal SubtotalIqd { get; set; }
    public decimal DiscountAmountUsd { get; set; }
    public decimal DiscountAmountIqd { get; set; }
    public string? CouponCode { get; set; }

    public string Status { get; set; } = "PendingPayment";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}
