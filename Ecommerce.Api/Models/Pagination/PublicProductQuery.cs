namespace Ecommerce.Api.Models.Products;

public class PublicProductQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Q { get; set; }
    public string? Brand { get; set; }
    public string? Sort { get; set; } = "new";
}
