// Ecommerce.Api/Domain/Entities/Payment.cs
namespace Ecommerce.Api.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Stripe / PayPal ...
    public string Provider { get; set; } = "Stripe";

    // Pending, Succeeded, Failed
    public string Status { get; set; } = "Pending";

    // رقم العملية عند مزود الدفع (PaymentIntentId مثلا)
    public string ProviderRef { get; set; } = "";

    public decimal AmountUsd { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
