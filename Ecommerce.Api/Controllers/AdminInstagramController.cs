using System.Net;
using System.Text.Json;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/instagram")]
[Authorize(Roles = "Admin")]
public class AdminInstagramController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly HttpClient _http;

    public AdminInstagramController(AppDbContext db, IWebHostEnvironment env, HttpClient http)
    {
        _db = db;
        _env = env;
        _http = http;
    }

    public record ImportRequest(
        string igUserId,
        string accessToken,
        int? maxItems = null,
        bool publish = true
    );

    public record ImportResult(int imported, int skipped, int errors);

    /// <summary>
    /// يسحب بوستات الانستغرام من IG Graph API ويحوّلها لمنتجات.
    /// ملاحظة: نخزّن الصور محلياً داخل wwwroot/uploads حتى ما تنتهي صلاحية media_url.
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.igUserId) || string.IsNullOrWhiteSpace(req.accessToken))
            return BadRequest(new { message = "igUserId و accessToken مطلوبة" });

        var max = req.maxItems.HasValue && req.maxItems.Value > 0 ? req.maxItems.Value : 200;

        int imported = 0, skipped = 0, errors = 0;
        string? nextUrl = BuildMediaListUrl(req.igUserId, req.accessToken);

        while (!string.IsNullOrWhiteSpace(nextUrl) && imported + skipped < max)
        {
            using var resp = await _http.GetAsync(nextUrl);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await SafeRead(resp);
                return StatusCode((int)resp.StatusCode, new { message = "Instagram fetch failed", status = (int)resp.StatusCode, body });
            }

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var data = doc.RootElement.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Array ? d : default;
            if (data.ValueKind == JsonValueKind.Undefined) break;

            foreach (var item in data.EnumerateArray())
            {
                if (imported + skipped >= max) break;

                try
                {
                    var mediaId = item.GetProperty("id").GetString() ?? "";
                    if (string.IsNullOrWhiteSpace(mediaId)) { skipped++; continue; }

                    // نستخدم slug ثابت يمنع التكرار
                    var slug = $"ig-{mediaId}";
                    var exists = await _db.Products.AnyAsync(p => p.Slug == slug);
                    if (exists) { skipped++; continue; }

                    var caption = item.TryGetProperty("caption", out var c) ? c.GetString() : null;
                    var permalink = item.TryGetProperty("permalink", out var pl) ? pl.GetString() : null;
                    var mediaType = item.TryGetProperty("media_type", out var mt) ? mt.GetString() : null;
                    var mediaUrl = item.TryGetProperty("media_url", out var mu) ? mu.GetString() : null;

                    var (title, description) = BuildTitleAndDescription(caption, permalink);
                    var priceUsd = TryExtractPriceUsd(caption);

                    var product = new Product
                    {
                        Title = title,
                        Slug = slug,
                        Description = description,
                        PriceUsd = priceUsd,
                        IsPublished = req.publish,
                    };

                    _db.Products.Add(product);
                    await _db.SaveChangesAsync();

                    // صور
                    var urls = new List<string>();
                    if (string.Equals(mediaType, "CAROUSEL_ALBUM", StringComparison.OrdinalIgnoreCase))
                    {
                        urls.AddRange(await FetchCarouselUrls(mediaId, req.accessToken));
                    }
                    else if (string.Equals(mediaType, "IMAGE", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(mediaUrl))
                    {
                        urls.Add(mediaUrl!);
                    }
                    else
                    {
                        // VIDEO أو غيره: نترك بدون صور
                    }

                    if (urls.Count > 0)
                        await DownloadAndAttachImages(product.Id, urls);

                    imported++;
                }
                catch
                {
                    errors++;
                }
            }

            nextUrl = null;
            if (doc.RootElement.TryGetProperty("paging", out var paging)
                && paging.TryGetProperty("next", out var next)
                && next.ValueKind == JsonValueKind.String)
            {
                nextUrl = next.GetString();
            }
        }

        return Ok(new ImportResult(imported, skipped, errors));
    }

    /// <summary>
    /// يحذف كل المنتجات المستوردة من الانستغرام (اللي Slug يبدأ بـ ig-)
    /// </summary>
    [HttpDelete("imported")]
    public async Task<IActionResult> DeleteImported()
    {
        var items = await _db.Products
            .Where(p => p.Slug.StartsWith("ig-"))
            .Select(p => p.Id)
            .ToListAsync();

        if (items.Count == 0) return Ok(new { deleted = 0 });

        // حذف الصور من الداتابيس وملفاتها
        foreach (var id in items)
        {
            var imgs = await _db.ProductImages.Where(x => x.ProductId == id).ToListAsync();
            foreach (var img in imgs)
            {
                TryDeleteLocalFile(img.Url);
            }
            _db.ProductImages.RemoveRange(imgs);
        }

        var products = await _db.Products.Where(p => items.Contains(p.Id)).ToListAsync();
        _db.Products.RemoveRange(products);
        await _db.SaveChangesAsync();

        return Ok(new { deleted = products.Count });
    }

    private string BuildMediaListUrl(string igUserId, string accessToken)
    {
        // IG Graph API (Business) غالباً يستخدم graph.facebook.com
        var baseUrl = "https://graph.facebook.com/v19.0";
        var fields = "id,caption,media_type,media_url,permalink,timestamp";
        return $"{baseUrl}/{WebUtility.UrlEncode(igUserId)}/media?fields={WebUtility.UrlEncode(fields)}&limit=25&access_token={WebUtility.UrlEncode(accessToken)}";
    }

    private async Task<List<string>> FetchCarouselUrls(string mediaId, string accessToken)
    {
        var baseUrl = "https://graph.facebook.com/v19.0";
        var fields = "id,media_type,media_url";
        var url = $"{baseUrl}/{WebUtility.UrlEncode(mediaId)}/children?fields={WebUtility.UrlEncode(fields)}&limit=50&access_token={WebUtility.UrlEncode(accessToken)}";
        using var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return new List<string>();
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            return new List<string>();
        var outUrls = new List<string>();
        foreach (var it in data.EnumerateArray())
        {
            var mt = it.TryGetProperty("media_type", out var mtt) ? mtt.GetString() : null;
            var mu = it.TryGetProperty("media_url", out var muu) ? muu.GetString() : null;
            if (string.Equals(mt, "IMAGE", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(mu))
                outUrls.Add(mu!);
        }
        return outUrls;
    }

    private async Task DownloadAndAttachImages(Guid productId, List<string> urls)
    {
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var dir = Path.Combine(webRoot, "uploads", "products", productId.ToString());
        Directory.CreateDirectory(dir);

        var maxSort = await _db.ProductImages
            .Where(x => x.ProductId == productId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? -1;

        foreach (var remote in urls.Distinct())
        {
            try
            {
                using var resp = await _http.GetAsync(remote);
                if (!resp.IsSuccessStatusCode) continue;

                var bytes = await resp.Content.ReadAsByteArrayAsync();
                if (bytes.Length == 0) continue;

                var ext = GuessExt(resp.Content.Headers.ContentType?.MediaType) ?? ".jpg";
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var full = Path.Combine(dir, fileName);
                await System.IO.File.WriteAllBytesAsync(full, bytes);

                var publicPath = $"/uploads/products/{productId}/{fileName}";
                var publicUrl = $"{Request.Scheme}://{Request.Host}{publicPath}";

                var img = new ProductImage
                {
                    ProductId = productId,
                    Url = publicUrl,
                    Alt = "instagram",
                    SortOrder = ++maxSort
                };
                _db.ProductImages.Add(img);
            }
            catch
            {
                // ignore single image errors
            }
        }

        await _db.SaveChangesAsync();
    }

    private void TryDeleteLocalFile(string url)
    {
        try
        {
            // نحول URL كامل لـ path نسبي
            var uri = Uri.TryCreate(url, UriKind.Absolute, out var u) ? u : null;
            var path = uri?.AbsolutePath ?? url;
            if (!path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return;

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

            var local = Path.Combine(webRoot, path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(local)) System.IO.File.Delete(local);
        }
        catch { }
    }

    private static string? GuessExt(string? mediaType)
    {
        return mediaType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => null
        };
    }

    private static (string title, string description) BuildTitleAndDescription(string? caption, string? permalink)
    {
        var cap = (caption ?? "").Trim();
        var firstLine = cap.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        var title = string.IsNullOrWhiteSpace(firstLine) ? "Instagram product" : firstLine;
        if (title.Length > 80) title = title[..80];

        var desc = string.IsNullOrWhiteSpace(cap) ? "Imported from Instagram" : cap;
        if (!string.IsNullOrWhiteSpace(permalink))
            desc += $"\n\nSource: {permalink}";
        return (title, desc);
    }

    // سعر اختياري من الكابشن: اذا ماكو يرجع 0 (والواجهة تعرضه كـ "-")
    private static decimal TryExtractPriceUsd(string? caption)
    {
        if (string.IsNullOrWhiteSpace(caption)) return 0;
        var text = caption;

        // يبحث عن $12.34 أو 12$ أو 12 usd
        var m = System.Text.RegularExpressions.Regex.Match(text, @"\$\s*(\d+(?:\.\d{1,2})?)");
        if (!m.Success) m = System.Text.RegularExpressions.Regex.Match(text, @"(\d+(?:\.\d{1,2})?)\s*(?:\$|usd|USD)\b");
        if (m.Success && decimal.TryParse(m.Groups[1].Value, out var v)) return v;

        return 0;
    }

    private static async Task<string?> SafeRead(HttpResponseMessage resp)
    {
        try { return await resp.Content.ReadAsStringAsync(); }
        catch { return null; }
    }
}
