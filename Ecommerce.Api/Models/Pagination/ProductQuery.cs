namespace Ecommerce.Api.Models.Products;

public class ProductQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Q { get; set; }
    // ✅ فلترة حسب البراند
    public string? Brand { get; set; }
    public string? Sort { get; set; } = "new";
}
