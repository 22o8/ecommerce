using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Ecommerce.Api.Infrastructure.Images;

public sealed record OptimizedUpload(MemoryStream Stream, string ContentType, string Extension, bool Optimized);

public static class ImageOptimizer
{
    public const int MaxDimension = 1920;
    public const int WebpQuality = 80;

    public static async Task<OptimizedUpload> OptimizeImageToWebpAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null) throw new ArgumentNullException(nameof(file));

        var contentType = file.ContentType ?? string.Empty;
        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var isImage =
            contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
            IsKnownImageExtension(ext);

        if (!isImage)
        {
            var original = new MemoryStream();
            await using var s = file.OpenReadStream();
            await s.CopyToAsync(original, ct);
            original.Position = 0;
            return new OptimizedUpload(original, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType, ext.ToLowerInvariant(), false);
        }

        try
        {
            await using var input = file.OpenReadStream();
            using var image = await Image.LoadAsync(input, ct);

            if (image.Width > MaxDimension || image.Height > MaxDimension)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(MaxDimension, MaxDimension),
                    Mode = ResizeMode.Max
                }));
            }

            var output = new MemoryStream();
            await image.SaveAsWebpAsync(output, new WebpEncoder { Quality = WebpQuality }, ct);
            output.Position = 0;
            return new OptimizedUpload(output, "image/webp", ".webp", true);
        }
        catch
        {
            // لا تفشل الرفع بسبب صورة غريبة أو تالفة. ارفع الأصل بدل ظهور image too large/processing error.
            var fallback = new MemoryStream();
            await using var s = file.OpenReadStream();
            await s.CopyToAsync(fallback, ct);
            fallback.Position = 0;
            return new OptimizedUpload(fallback, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType, ext.ToLowerInvariant(), false);
        }
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
