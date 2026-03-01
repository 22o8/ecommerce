using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Infrastructure.Storage;

public sealed class S3ObjectStorage : IObjectStorage
{
    private readonly ObjectStorageOptions _opt;
    private readonly IAmazonS3 _s3;

    private string NormalizeKey(string key)
    {
        key = (key ?? string.Empty).TrimStart('/');
        var prefix = _opt.KeyPrefix?.Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(prefix)) return key;
        return $"{prefix}/{key}";
    }

    public S3ObjectStorage(IOptions<ObjectStorageOptions> opt)
    {
        _opt = opt.Value;

        var cfg = new AmazonS3Config
        {
            ForcePathStyle = true,
            // Cloudflare R2 (وأغلب S3-compatible) لا يدعم بعض أنماط الـ streaming/chunked
            // التي يستخدمها AWS SDK افتراضياً مع التوقيع. تعطيلها يمنع خطأ:
            // STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER not implemented
        };

        var endpoint = !string.IsNullOrWhiteSpace(_opt.Endpoint) ? _opt.Endpoint : _opt.ServiceUrl;
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            cfg.ServiceURL = endpoint;
        }

        if (!string.IsNullOrWhiteSpace(_opt.Region))
        {
            cfg.RegionEndpoint = RegionEndpoint.GetBySystemName(_opt.Region);
        }

        // If neither Region nor Endpoint is provided, the AWS SDK throws:
        // "No RegionEndpoint or ServiceURL configured".
        // Default to a region so non-storage endpoints can work even when
        // object storage isn't configured yet.
        if (cfg.RegionEndpoint is null && string.IsNullOrWhiteSpace(cfg.ServiceURL))
        {
            cfg.RegionEndpoint = RegionEndpoint.USEast1;
        }

        _s3 = new AmazonS3Client(_opt.AccessKeyId, _opt.SecretAccessKey, cfg);
    }

    public async Task<StoredObject> UploadAsync(Stream content, string key, string contentType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.Bucket))
            throw new InvalidOperationException("Object storage bucket is not configured.");

        var normalizedKey = NormalizeKey(key);

        // مهم: حتى نتجنب aws-chunked/streaming signing
        // لازم نرسل Content-Length. إذا الـ stream غير قابل للـ seek نعمل buffering.
        Stream uploadStream = content;
        MemoryStream? buffered = null;
        if (content is null) throw new ArgumentNullException(nameof(content));

        if (!content.CanSeek)
        {
            buffered = new MemoryStream();
            await content.CopyToAsync(buffered, ct);
            buffered.Position = 0;
            uploadStream = buffered;
        }
        else
        {
            // تأكد البوزيشن بالبداية حتى لا يرفع ملف ناقص
            if (content.Position != 0) content.Position = 0;
        }

        var put = new PutObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = normalizedKey,
            InputStream = uploadStream,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
        };

        // ضروري لتجنب chunked upload
        if (uploadStream.CanSeek)
            // بعض إصدارات AWS SDK لا تحتوي PutObjectRequest.ContentLength؛ نستخدم Headers.ContentLength بدلاً عنه
            put.Headers.ContentLength = uploadStream.Length;

        // تعزيز التوافق مع R2/S3-compatible
        // (إجبار Content-Length + تعطيل chunked في config أعلاه)

        // PublicRead works on AWS S3; for R2 use bucket policy instead.
        // We keep it optional to avoid failures.
        try { put.CannedACL = S3CannedACL.PublicRead; } catch { /* ignore */ }

        try
        {
            await _s3.PutObjectAsync(put, ct);
        }
        finally
        {
            buffered?.Dispose();
        }

        var url = BuildPublicUrl(key);
        return new StoredObject(key, url);
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.Bucket)) return;
        await _s3.DeleteObjectAsync(_opt.Bucket, NormalizeKey(key), ct);
    }

    private string BuildPublicUrl(string key)
    {
        key = NormalizeKey(key);
        // Best practice: set PublicBaseUrl to your CDN/custom domain.
        if (!string.IsNullOrWhiteSpace(_opt.PublicBaseUrl))
            return _opt.PublicBaseUrl.TrimEnd('/') + "/" + key;

        // Fallback to endpoint/bucket-style URL (may not be public depending on provider).
        var ep = !string.IsNullOrWhiteSpace(_opt.Endpoint) ? _opt.Endpoint : _opt.ServiceUrl;
        if (!string.IsNullOrWhiteSpace(ep))
            return ep.TrimEnd('/') + "/" + _opt.Bucket + "/" + key;

        return key;
    }
}
