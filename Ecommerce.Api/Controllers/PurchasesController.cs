using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PurchasesController(AppDbContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idStr, out userId);
    }

    // GET /api/purchases/product/{orderId}
    [HttpGet("product/{orderId:guid}")]
    public async Task<IActionResult> GetProductPurchase(Guid orderId)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized("Invalid token");

        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) return NotFound("Order not found");

        var item = order.Items.FirstOrDefault(i => i.ItemType == "DigitalProduct" && i.ProductId != null);
        if (item?.ProductId == null) return BadRequest("Not a product order");

        var asset = await _db.ProductAssets.AsNoTracking().FirstOrDefaultAsync(a => a.ProductId == item.ProductId.Value);

        // يرجع معلومات حتى لو ماكو Asset (بس يحذرك)
        string? downloadToken = null;

        if (order.Status == "Paid" && asset != null)
        {
            // أصدر Token مؤقت جديد كل مرة
            var token = new Domain.Entities.DownloadToken
            {
                OrderId = order.Id,
                ProductId = item.ProductId.Value,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _db.DownloadTokens.Add(token);
            await _db.SaveChangesAsync();

            downloadToken = token.Token;
        }

        return Ok(new
        {
            orderId = order.Id,
            orderStatus = order.Status,
            product = new { id = item.ProductId, title = item.Product?.Title, slug = item.Product?.Slug },
            asset = asset == null ? null : new
            {
                asset.StorageType,
                asset.Instructions,
                asset.SupportContact
            },
            download = order.Status == "Paid" && asset != null
                ? new { token = downloadToken, url = $"/api/download/{downloadToken}" }
                : null
        });
    }
}
