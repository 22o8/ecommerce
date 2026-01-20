namespace Ecommerce.Api.Domain.Entities;

public class ServiceRequestAnswer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceRequestId { get; set; }
    public ServiceRequest ServiceRequest { get; set; } = null!;

    public Guid RequirementId { get; set; }
    public ServiceRequirementTemplate Requirement { get; set; } = null!;

    public string Answer { get; set; } = "";
}
