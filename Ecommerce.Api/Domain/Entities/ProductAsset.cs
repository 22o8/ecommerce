namespace Ecommerce.Api.Domain.Entities;

public class ProductAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // "ExternalUrl" أو "LocalFile"
    public string StorageType { get; set; } = "ExternalUrl";

    // إذا ExternalUrl => هنا تحط رابط التحميل (خصوصي/عام)
    public string? ExternalUrl { get; set; }

    // إذا LocalFile => مسار الملف بالسيرفر (اختياري لاحقاً)
    public string? LocalPath { get; set; }

    // تعليمات تظهر بعد الشراء
    public string Instructions { get; set; } = "";

    // معلومات دعم
    public string SupportContact { get; set; } = ""; // مثال: WhatsApp: +964... / Telegram: ...

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
