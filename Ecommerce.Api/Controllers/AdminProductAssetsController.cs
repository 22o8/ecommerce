// Ecommerce.Api/Controllers/AdminProductAssetsController.cs
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/product-assets")]
[Authorize(Roles = "Admin")]
public class AdminProductAssetsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminProductAssetsController(AppDbContext db)
    {
        _db = db;
    }

    // PUT /api/admin/product-assets/{productId}
    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> Upsert(Guid productId, [FromBody] UpsertProductAssetRequest req)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return NotFound("Product not found");

        var asset = await _db.ProductAssets.FirstOrDefaultAsync(a => a.ProductId == productId);

        if (asset == null)
        {
            asset = new ProductAsset { ProductId = productId };
            _db.ProductAssets.Add(asset);
        }

        asset.StorageType = (req.StorageType ?? "ExternalUrl").Trim();
        asset.ExternalUrl = req.ExternalUrl?.Trim();
        asset.LocalPath = req.LocalPath?.Trim();
        asset.Instructions = req.Instructions?.Trim() ?? "";
        asset.SupportContact = req.SupportContact?.Trim() ?? "";

        // تحقق بسيط
        if (asset.StorageType == "ExternalUrl" && string.IsNullOrWhiteSpace(asset.ExternalUrl))
            return BadRequest("ExternalUrl is required when StorageType=ExternalUrl");

        await _db.SaveChangesAsync();

        return Ok(new
        {
            asset.Id,
            asset.ProductId,
            asset.StorageType,
            asset.ExternalUrl,
            asset.Instructions,
            asset.SupportContact
        });
    }

    // GET /api/admin/product-assets/{productId}
    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> Get(Guid productId)
    {
        var asset = await _db.ProductAssets.AsNoTracking().FirstOrDefaultAsync(a => a.ProductId == productId);
        if (asset == null) return NotFound();
        return Ok(asset);
    }
}

public class UpsertProductAssetRequest
{
    public string? StorageType { get; set; } = "ExternalUrl";
    public string? ExternalUrl { get; set; }
    public string? LocalPath { get; set; }
    public string? Instructions { get; set; }
    public string? SupportContact { get; set; }
}
