namespace Ecommerce.Api.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public string Provider { get; set; } = "Mock";
    public string Status { get; set; } = "Pending";
    public string ProviderRef { get; set; } = "";

    // (قديم)
    public decimal AmountUsd { get; set; }

    // المعتمد
    public decimal AmountIqd { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
