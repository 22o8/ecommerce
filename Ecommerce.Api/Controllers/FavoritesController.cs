using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FavoritesController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub")
                 ?? User.FindFirstValue("userId");

        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException("Missing user id claim");

        return Guid.Parse(id);
    }

    // GET /api/favorites/my
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> MyFavorites()
    {
        var userId = GetUserId();

        var items = await _db.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Join(_db.Products,
                f => f.ProductId,
                p => p.Id,
                (f, p) => new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Brand,
                    p.PriceIqd,
                    p.PriceUsd,
                    favoritedAt = f.CreatedAt
                })
            .ToListAsync();

        return Ok(items);
    }

    // POST /api/favorites/toggle/{productId}
    [Authorize]
    [HttpPost("toggle/{productId:guid}")]
    public async Task<IActionResult> Toggle(Guid productId)
    {
        var userId = GetUserId();

        var existing = await _db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

        if (existing != null)
        {
            _db.Favorites.Remove(existing);
            await _db.SaveChangesAsync();
            return Ok(new { isFavorited = false });
        }

        // ensure product exists
        var exists = await _db.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return NotFound();

        _db.Favorites.Add(new Domain.Entities.Favorite
        {
            UserId = userId,
            ProductId = productId
        });

        await _db.SaveChangesAsync();
        return Ok(new { isFavorited = true });
    }
}
