namespace Ecommerce.Api.Infrastructure.Storage;

public interface IObjectStorage
{
    // Keep parameter name as `cancellationToken` so callers can use named arguments.
    Task<StoredObject> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}

public sealed record StoredObject(string Key, string Url);
