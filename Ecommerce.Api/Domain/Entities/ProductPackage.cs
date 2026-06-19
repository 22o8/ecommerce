namespace Ecommerce.Api.Domain.Entities;

public class ProductPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NameAr { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string Slug { get; set; } = "";
    public string ShortDescription { get; set; } = "";
    public string SeoDescription { get; set; } = "";
    public string Note { get; set; } = "";
    public string CoverUrl { get; set; } = "";
    public string MediaType { get; set; } = "image";
    public decimal OriginalPriceIqd { get; set; } = 0m;
    public decimal FinalPriceIqd { get; set; } = 0m;
    public string Category { get; set; } = "";
    public string ProblemCategory { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool ShowInSlider { get; set; } = false;
    public string SliderPlacement { get; set; } = "packages";
    public int SortOrder { get; set; } = 0;
    public int SoldCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ProductPackageItem> Items { get; set; } = new();
}
