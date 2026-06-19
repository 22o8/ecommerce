using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Infrastructure.Storage;

public sealed class LocalObjectStorage : IObjectStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly ObjectStorageOptions _opt;

    public LocalObjectStorage(IWebHostEnvironment env, IOptions<ObjectStorageOptions> opt)
    {
        _env = env;
        _opt = opt.Value;
    }

    public async Task<StoredObject> UploadAsync(Stream content, string key, string contentType, CancellationToken ct = default)
    {
        // key example: products/{productId}/{fileName}.ext
        var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
        var fullPath = Path.Combine(uploadsRoot, key.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, ct);

        // Return a URL that frontend can load.
        // If PublicBaseUrl is set, use it. Otherwise keep legacy relative path.
        var relative = "/uploads/" + key.Replace("\\", "/");
        var url = string.IsNullOrWhiteSpace(_opt.PublicBaseUrl)
            ? relative
            : _opt.PublicBaseUrl.TrimEnd('/') + relative;

        return new StoredObject(key, url);
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
        var fullPath = Path.Combine(uploadsRoot, key.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
