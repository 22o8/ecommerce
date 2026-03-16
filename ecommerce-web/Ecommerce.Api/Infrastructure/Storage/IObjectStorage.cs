namespace Ecommerce.Api.Infrastructure.Storage;

public interface IObjectStorage
{
    Task<StoredObject> UploadAsync(Stream content, string key, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}

public sealed record StoredObject(string Key, string Url);
