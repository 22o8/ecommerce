// Ecommerce.Api/Domain/Entities/Product.cs
namespace Ecommerce.Api.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>
    /// السعر بالدينار العراقي (المعتمد في الواجهة).
    /// </summary>
    public decimal PriceIqd { get; set; }

    /// <summary>
    /// (قديم) كان يستخدم بالدولار. أبقيناه للتوافق الخلفي فقط.
    /// </summary>
    public decimal PriceUsd { get; set; }

    // Brand / فهرسة
    public string Brand { get; set; } = "Unspecified";
    public bool IsPublished { get; set; } = true;

    // Admin-controlled flag to show this product on home page (Featured Products)
    public bool IsFeatured { get; set; } = false;

    // تقييم (يدوي/إداري حالياً)
    public decimal RatingAvg { get; set; } = 0m;   // 0..5
    public int RatingCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProductAsset? Asset { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
