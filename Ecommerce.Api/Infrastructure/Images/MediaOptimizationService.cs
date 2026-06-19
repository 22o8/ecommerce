using Ecommerce.Api.Infrastructure.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Ecommerce.Api.Infrastructure.Images;

public sealed record MediaOptimizeResult(string Key, string Url, int Width, int Height, long Bytes, string ContentType);

public sealed class MediaOptimizationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IObjectStorage _storage;

    public const int MaxLarge = 1200;
    public const int MaxMedium = 600;
    public const int MaxThumb = 150;
    public const int Quality = 65;

    public MediaOptimizationService(IHttpClientFactory httpClientFactory, IObjectStorage storage)
    {
        _httpClientFactory = httpClientFactory;
        _storage = storage;
    }

    public async Task<MediaOptimizeResult?> OptimizeRemoteImageAsync(string? url, string keyPrefix, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (url.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)) return null;
        if (url.Contains("/optimized/", StringComparison.OrdinalIgnoreCase)) return null;

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(25);

        await using var remote = await client.GetStreamAsync(url, ct);
        using var image = await Image.LoadAsync(remote, ct);

        var id = Guid.NewGuid().ToString("N");
        var baseKey = $"optimized/{keyPrefix.Trim('/').Trim()}/{DateTime.UtcNow:yyyyMMdd}/{id}";

        // نرفع 3 نسخ فعلية للاستخدام المستقبلي، ونرجع medium للواجهة حتى يقل الحجم مباشرة.
        var thumb = await SaveVariantAsync(image, MaxThumb, $"{baseKey}-thumb.webp", ct);
        var medium = await SaveVariantAsync(image, MaxMedium, $"{baseKey}-medium.webp", ct);
        await SaveVariantAsync(image, MaxLarge, $"{baseKey}-large.webp", ct);

        return medium ?? thumb;
    }

    public static async Task<OptimizedUpload> OptimizeUploadStrictAsync(IFormFile file, int maxDimension = MaxLarge, int quality = Quality, CancellationToken ct = default)
    {
        if (file is null) throw new ArgumentNullException(nameof(file));
        var contentType = file.ContentType ?? string.Empty;
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var isImage = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || IsKnownImageExtension(ext);
        if (!isImage)
        {
            var original = new MemoryStream();
            await using var s = file.OpenReadStream();
            await s.CopyToAsync(original, ct);
            original.Position = 0;
            return new OptimizedUpload(original, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType, ext.ToLowerInvariant(), false);
        }

        await using var input = file.OpenReadStream();
        using var image = await Image.LoadAsync(input, ct);
        ResizeIfNeeded(image, maxDimension);

        var output = new MemoryStream();
        await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = quality }, ct);
        output.Position = 0;
        return new OptimizedUpload(output, "image/webp", ".webp", true);
    }

    private async Task<MediaOptimizeResult?> SaveVariantAsync(Image source, int max, string key, CancellationToken ct)
    {
        using var clone = source.Clone(ctx => { });
        ResizeIfNeeded(clone, max);

        await using var ms = new MemoryStream();
        await clone.SaveAsWebpAsync(ms, new WebpEncoder { Quality = Quality }, ct);
        ms.Position = 0;
        var bytes = ms.Length;
        var stored = await _storage.UploadAsync(ms, key, "image/webp", ct);
        return new MediaOptimizeResult(stored.Key, stored.Url, clone.Width, clone.Height, bytes, "image/webp");
    }

    private static void ResizeIfNeeded(Image image, int max)
    {
        if (image.Width <= max && image.Height <= max) return;
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(max, max),
            Mode = ResizeMode.Max
        }));
    }

    private static bool IsKnownImageExtension(string ext)
    {
        return ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".avif", StringComparison.OrdinalIgnoreCase);
    }
}
