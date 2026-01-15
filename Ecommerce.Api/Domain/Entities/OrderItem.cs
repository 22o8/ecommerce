// Ecommerce.Api/Domain/Entities/Payment.cs
namespace Ecommerce.Api.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // DigitalProduct | Service
    public string ItemType { get; set; } = "DigitalProduct";

    // للمنتج: ProductId
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    // للخدمة: ServiceId + PackageId + ServiceRequestId (اختياري)
    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; }

    public Guid? PackageId { get; set; }
    public ServicePackage? Package { get; set; }

    public Guid? ServiceRequestId { get; set; }
    public ServiceRequest? ServiceRequest { get; set; }

    public decimal UnitPriceUsd { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal LineTotalUsd { get; set; }
}
