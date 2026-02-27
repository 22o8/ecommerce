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

        // Support both "ServiceUrl" and legacy "Endpoint".
        var serviceUrl = _opt.ServiceUrl;
        if (string.IsNullOrWhiteSpace(serviceUrl)) serviceUrl = _opt.Endpoint;

        var cfg = new AmazonS3Config
        {
            ForcePathStyle = true
        };

        // For S3-compatible providers (Cloudflare R2, MinIO, etc.) you typically set ServiceURL.
        if (!string.IsNullOrWhiteSpace(serviceUrl))
            cfg.ServiceURL = serviceUrl;

        // If Region is not provided, keep it unset. For R2, "auto" is commonly used.
        if (!string.IsNullOrWhiteSpace(_opt.Region))
        {
            // "auto" isn't a real AWS region endpoint, but the AWS SDK uses AuthenticationRegion
            // for signing when ServiceURL is set. We handle both cases.
            if (string.Equals(_opt.Region, "auto", StringComparison.OrdinalIgnoreCase))
                cfg.AuthenticationRegion = "auto";
            else
                cfg.RegionEndpoint = RegionEndpoint.GetBySystemName(_opt.Region);
        }
        else
        {
            // If ServiceURL is set but Region is not, set a safe default for signing with R2.
            // This prevents: AmazonClientException: No RegionEndpoint or ServiceURL configured
            // when misconfigured env vars are present.
            if (!string.IsNullOrWhiteSpace(cfg.ServiceURL))
                cfg.AuthenticationRegion = "auto";
        }

        if (string.IsNullOrWhiteSpace(cfg.ServiceURL) && cfg.RegionEndpoint is null)
            throw new InvalidOperationException(
                "ObjectStorage is misconfigured: set ObjectStorage:ServiceUrl (or Endpoint) and/or ObjectStorage:Region.");

        if (string.IsNullOrWhiteSpace(_opt.AccessKeyId) || string.IsNullOrWhiteSpace(_opt.SecretAccessKey))
            throw new InvalidOperationException(
                "ObjectStorage credentials are missing: set ObjectStorage:AccessKeyId and ObjectStorage:SecretAccessKey.");

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

        var serviceUrl = _opt.ServiceUrl;
        if (string.IsNullOrWhiteSpace(serviceUrl)) serviceUrl = _opt.Endpoint;

        // Fallback to endpoint/bucket-style URL (may not be public depending on provider).
        if (!string.IsNullOrWhiteSpace(serviceUrl))
            return serviceUrl.TrimEnd('/') + "/" + _opt.Bucket + "/" + key;

        return key;
    }
}
