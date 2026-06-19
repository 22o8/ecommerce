using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/activities")]
[Authorize(Roles = "Admin")]
public class AdminActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminActivitiesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int take = 100, [FromQuery] string? entityType = null, [FromQuery] string? action = null)
    {
        take = Math.Clamp(take, 1, 300);
        var q = _db.AdminActivities.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(x => x.EntityType.ToLower() == entityType.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(action)) q = q.Where(x => x.Action.ToLower() == action.Trim().ToLower());

        var items = await q.OrderByDescending(x => x.CreatedAtUtc).Take(take).Select(x => new
        {
            x.Id,
            x.AdminUserId,
            x.AdminEmail,
            x.Action,
            x.EntityType,
            x.EntityId,
            x.Title,
            x.Details,
            x.MetadataJson,
            x.CreatedAtUtc
        }).ToListAsync();
        return Ok(new { items });
    }

    [HttpGet("category-health")]
    public async Task<IActionResult> CategoryHealth()
    {
        var products = await _db.Products.AsNoTracking().Select(p => new
        {
            p.Id,
            p.Title,
            p.Slug,
            p.Category,
            p.SubCategory,
            p.ProblemCategory,
            p.ProblemSubCategory,
            p.IsPublished
        }).ToListAsync();
        var cats = await _db.Categories.AsNoTracking().Select(c => new { c.Id, c.Key, c.Section, c.ParentId, c.IsActive }).ToListAsync();
        var regularParents = cats.Where(c => c.Section.ToLower() == "regular" && c.ParentId == null && c.IsActive).ToDictionary(c => c.Key.ToLower(), c => c.Id);
        var problemParents = cats.Where(c => c.Section.ToLower() == "problem" && c.ParentId == null && c.IsActive).ToDictionary(c => c.Key.ToLower(), c => c.Id);
        var regularChildren = cats.Where(c => c.Section.ToLower() == "regular" && c.ParentId != null && c.IsActive).Select(c => ($"{c.ParentId}:{c.Key}".ToLower())).ToHashSet();
        var problemChildren = cats.Where(c => c.Section.ToLower() == "problem" && c.ParentId != null && c.IsActive).Select(c => ($"{c.ParentId}:{c.Key}".ToLower())).ToHashSet();

        var issues = new List<object>();
        foreach (var p in products)
        {
            var category = (p.Category ?? "").Trim().ToLower();
            var sub = (p.SubCategory ?? "").Trim().ToLower();
            var problem = (p.ProblemCategory ?? "").Trim().ToLower();
            var problemSub = (p.ProblemSubCategory ?? "").Trim().ToLower();
            var productIssues = new List<string>();

            if (string.IsNullOrWhiteSpace(category)) productIssues.Add("لا يوجد تصنيف رئيسي");
            else if (!regularParents.TryGetValue(category, out var parentId)) productIssues.Add("التصنيف الرئيسي غير موجود أو غير فعال");
            else if (!string.IsNullOrWhiteSpace(sub) && !regularChildren.Contains($"{parentId}:{sub}".ToLower())) productIssues.Add("التصنيف الدقيق غير موجود تحت هذا التصنيف الرئيسي");

            if (!string.IsNullOrWhiteSpace(problem))
            {
                if (!problemParents.TryGetValue(problem, out var problemParentId)) productIssues.Add("تصنيف حل المشكلة غير موجود أو غير فعال");
                else if (!string.IsNullOrWhiteSpace(problemSub) && !problemChildren.Contains($"{problemParentId}:{problemSub}".ToLower())) productIssues.Add("القسم الدقيق لحل المشكلة غير موجود تحت تصنيف المشكلة");
            }
            else if (!string.IsNullOrWhiteSpace(problemSub)) productIssues.Add("يوجد قسم دقيق للمشكلة بدون تصنيف مشكلة رئيسي");

            if (productIssues.Count > 0)
            {
                issues.Add(new { p.Id, p.Title, p.Slug, p.Category, p.SubCategory, p.ProblemCategory, p.ProblemSubCategory, p.IsPublished, issues = productIssues });
            }
        }

        return Ok(new { totalProducts = products.Count, issuesCount = issues.Count, issues });
    }
}
