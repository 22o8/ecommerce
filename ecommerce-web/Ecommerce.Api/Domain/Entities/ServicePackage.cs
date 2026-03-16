namespace Ecommerce.Api.Domain.Entities;

public class ServicePackage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    // مثال: Basic / Standard / Premium
    public string Name { get; set; } = "Basic";

    // المعتمد
    public decimal PriceIqd { get; set; }

    // (قديم)
    public decimal PriceUsd { get; set; }

    public int DeliveryDays { get; set; } = 3;

    public string Features { get; set; } = ""; // نص بسيط: "ميزة1\nميزة2"
}
