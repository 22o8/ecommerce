namespace Ecommerce.Api.Infrastructure.Storage;

public sealed class ObjectStorageOptions
{
    // Provider: "Local" (default) or "S3"
    public string Provider { get; set; } = "Local";

    // S3 / R2 settings
    public string? Bucket { get; set; }
    public string? Region { get; set; } // optional (R2 can ignore)
    public string? Endpoint { get; set; }
    // دعم أسماء بيئة أقدم: ObjectStorage__ServiceUrl
    public string? ServiceUrl { get; set; } // e.g. https://<accountid>.r2.cloudflarestorage.com
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }

    // Public base url used to build image URLs (CDN/custom domain recommended)
    // Example: https://cdn.example.com
    public string? PublicBaseUrl { get; set; }
    // Optional prefix inside the bucket (e.g. "uploads").
    // If you set PublicBaseUrl to include a path (like .../uploads), also set KeyPrefix="uploads"
    // so stored keys match the generated public URLs.
    public string? KeyPrefix { get; set; }

}
