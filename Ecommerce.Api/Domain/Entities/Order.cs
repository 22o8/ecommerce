namespace Ecommerce.Api.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // PendingPayment, Paid, Cancelled, Refunded
    public string Status { get; set; } = "PendingPayment";

    public decimal TotalUsd { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}
