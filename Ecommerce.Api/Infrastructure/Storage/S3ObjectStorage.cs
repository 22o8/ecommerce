using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Infrastructure.Storage;

public sealed class S3ObjectStorage : IObjectStorage
{
    private readonly ObjectStorageOptions _opt;
    private readonly IAmazonS3 _s3;

    public S3ObjectStorage(IOptions<ObjectStorageOptions> opt)
    {
        _opt = opt.Value;

        var cfg = new AmazonS3Config
        {
            ForcePathStyle = true
        };

        if (!string.IsNullOrWhiteSpace(_opt.Endpoint))
        {
            cfg.ServiceURL = _opt.Endpoint;
        }

        if (!string.IsNullOrWhiteSpace(_opt.Region))
        {
            cfg.RegionEndpoint = RegionEndpoint.GetBySystemName(_opt.Region);
        }

        _s3 = new AmazonS3Client(_opt.AccessKeyId, _opt.SecretAccessKey, cfg);
    }

    public async Task<StoredObject> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.Bucket))
            throw new InvalidOperationException("Object storage bucket is not configured.");

        var put = new PutObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = key,
            InputStream = content,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
        };

        // PublicRead works on AWS S3; for R2 use bucket policy instead.
        // We keep it optional to avoid failures.
        try { put.CannedACL = S3CannedACL.PublicRead; } catch { /* ignore */ }

        await _s3.PutObjectAsync(put, cancellationToken);

        var url = BuildPublicUrl(key);
        return new StoredObject(key, url);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.Bucket)) return;
        await _s3.DeleteObjectAsync(_opt.Bucket, key, cancellationToken);
    }

    private string BuildPublicUrl(string key)
    {
        // Best practice: set PublicBaseUrl to your CDN/custom domain.
        if (!string.IsNullOrWhiteSpace(_opt.PublicBaseUrl))
            return _opt.PublicBaseUrl.TrimEnd('/') + "/" + key;

        // Fallback to endpoint/bucket-style URL (may not be public depending on provider).
        if (!string.IsNullOrWhiteSpace(_opt.Endpoint))
            return _opt.Endpoint.TrimEnd('/') + "/" + _opt.Bucket + "/" + key;

        return key;
    }
}
