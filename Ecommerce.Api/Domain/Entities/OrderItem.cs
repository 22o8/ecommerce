namespace Ecommerce.Api.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Either a product or a service/package-based line
    public string ItemType { get; set; } = "DigitalProduct";

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; }

    public Guid? PackageId { get; set; }
    public ServicePackage? Package { get; set; }

    public Guid? ServiceRequestId { get; set; }
    public ServiceRequest? ServiceRequest { get; set; }

    public int Quantity { get; set; } = 1;

    // (قديم) للتوافق
    public decimal UnitPriceUsd { get; set; }
    public decimal LineTotalUsd { get; set; }

    // المعتمد
    public decimal UnitPriceIqd { get; set; }
    public decimal LineTotalIqd { get; set; }
}
