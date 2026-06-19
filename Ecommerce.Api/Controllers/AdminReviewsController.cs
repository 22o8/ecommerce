using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminReviewsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 50 : pageSize;
        var q = _db.ProductReviews.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
            q = q.Where(x => x.Status == status);

        var totalCount = await q.CountAsync();
        var items = await q
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.ProductId,
                ProductTitle = _db.Products.Where(p => p.Id == r.ProductId).Select(p => p.Title).FirstOrDefault(),
                ProductSlug = _db.Products.Where(p => p.Id == r.ProductId).Select(p => p.Slug).FirstOrDefault(),
                r.Rating,
                r.Comment,
                r.ReviewerName,
                r.IsVerifiedPurchase,
                r.ImageUrlsJson,
                r.Status,
                r.CreatedAt,
                r.UpdatedAt,
                UserName = r.ReviewerName ?? _db.Users.Where(u => u.Id == r.UserId).Select(u => u.FullName).FirstOrDefault(),
            })
            .ToListAsync();
        return Ok(new { totalCount, items });
    }

    public record UpdateReviewRequest(string? Status, bool? IsVerifiedPurchase, string? ReviewerName);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewRequest req)
    {
        var r = await _db.ProductReviews.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound(new { message = "Review not found" });
        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            var status = req.Status.Trim();
            if (status is not ("Approved" or "Pending" or "Hidden"))
                return BadRequest(new { message = "Invalid status" });
            r.Status = status;
        }
        if (req.IsVerifiedPurchase.HasValue) r.IsVerifiedPurchase = req.IsVerifiedPurchase.Value;
        if (!string.IsNullOrWhiteSpace(req.ReviewerName)) r.ReviewerName = req.ReviewerName.Trim();
        r.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Review updated" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _db.ProductReviews.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound(new { message = "Review not found" });
        _db.ProductReviews.Remove(r);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Review deleted" });
    }
}
