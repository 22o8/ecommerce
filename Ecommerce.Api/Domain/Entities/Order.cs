namespace Ecommerce.Api.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // ✅ Nullable لتسهيل طلبات الزائر (WhatsApp Checkout) بدون حساب
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    // (قديم) للتوافق
    public decimal TotalUsd { get; set; }

    // المعتمد
    public decimal TotalIqd { get; set; }

    public string Status { get; set; } = "PendingPayment";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}
