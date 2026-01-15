namespace Ecommerce.Api.Domain.Entities;

public class Service
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";

    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ServicePackage> Packages { get; set; } = [];
    public List<ServiceRequirementTemplate> Requirements { get; set; } = [];
}
