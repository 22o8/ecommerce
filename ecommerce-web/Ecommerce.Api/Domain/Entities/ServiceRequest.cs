namespace Ecommerce.Api.Domain.Entities;

public class ServiceRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public Guid PackageId { get; set; }
    public ServicePackage Package { get; set; } = null!;

    // PendingPayment, Paid, InProgress, Delivered, Cancelled
    public string Status { get; set; } = "PendingPayment";

    // ملاحظات عامة من المستخدم
    public string Notes { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ServiceRequestAnswer> Answers { get; set; } = [];
}
