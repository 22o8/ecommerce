namespace Ecommerce.Api.Domain.Entities;

public class ProductPackageItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductPackageId { get; set; }
    public ProductPackage? ProductPackage { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
}
