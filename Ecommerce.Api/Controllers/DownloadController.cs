// Ecommerce.Api/Controllers/DownloadController.cs
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DownloadController : ControllerBase
{
    private readonly AppDbContext _db;

    public DownloadController(AppDbContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idStr, out userId);
    }

    // GET /api/download/{token}
    [HttpGet("{token}")]
    public async Task<IActionResult> Download(string token)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var t = await _db.DownloadTokens
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Token == token);

        if (t == null) return NotFound("Invalid token");
        if (t.UserId != userId) return Forbid();
        if (t.ExpiresAt < DateTime.UtcNow) return BadRequest("Token expired");
        if (t.Order.Status != "Paid") return BadRequest("Order not paid");

        var asset = await _db.ProductAssets.AsNoTracking().FirstOrDefaultAsync(a => a.ProductId == t.ProductId);
        if (asset == null) return NotFound("No asset linked to this product");

        // ExternalUrl: نعمل Redirect (أفضل بداية)
        if (asset.StorageType == "ExternalUrl" && !string.IsNullOrWhiteSpace(asset.ExternalUrl))
        {
            t.IsUsed = true;
            await _db.SaveChangesAsync();

            return Redirect(asset.ExternalUrl);
        }

        // LocalFile (لاحقاً): نقرأ الملف ونرجعه File()
        return BadRequest("LocalFile not implemented yet");
    }
}
