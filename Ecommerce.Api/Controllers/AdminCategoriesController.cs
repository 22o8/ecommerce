using System.Text.RegularExpressions;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Infrastructure.Storage;
using Ecommerce.Api.Infrastructure.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IObjectStorage _storage;

    public AdminCategoriesController(AppDbContext db, IObjectStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public record SaveCategoryRequest(
        string Key,
        string NameAr,
        string? NameEn,
        string? DescriptionAr,
        string? DescriptionEn,
        string? ImageUrl,
        int SortOrder,
        bool IsActive,
        string? Section,
        Guid? ParentId,
        bool HasDetailSections
    );

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? section = null, [FromQuery] Guid? parentId = null, [FromQuery] bool rootsOnly = false)
    {
        var normalizedSection = string.IsNullOrWhiteSpace(section) ? null : section.Trim().ToLowerInvariant();
        var query = _db.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(normalizedSection))
            query = query.Where(x => x.Section.ToLower() == normalizedSection);

        if (parentId.HasValue)
            query = query.Where(x => x.ParentId == parentId.Value);
        else if (rootsOnly)
            query = query.Where(x => x.ParentId == null);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameAr)
            .ToListAsync();

        return Ok(items.Select(x => new
        {
            x.Id,
            x.Key,
            x.NameAr,
            x.NameEn,
            x.DescriptionAr,
            x.DescriptionEn,
            x.ImageUrl,
            x.Section,
            x.ParentId,
            x.HasDetailSections,
            ChildCount = _db.Categories.Count(c => c.ParentId == x.Id),
            x.SortOrder,
            x.IsActive,
            x.CreatedAt,
            x.UpdatedAt
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveCategoryRequest req)
    {
        var key = Slugify(req.Key);
        if (string.IsNullOrWhiteSpace(key)) key = Slugify(req.NameEn) ?? Slugify(req.NameAr);
        if (string.IsNullOrWhiteSpace(key)) return BadRequest(new { message = "Category key is required" });
        if (string.IsNullOrWhiteSpace(req.NameAr)) return BadRequest(new { message = "Arabic name is required" });

        var normalizedSection = NormalizeSection(req.Section);
        Guid? normalizedParentId = null;
        if (req.ParentId.HasValue)
        {
            var parent = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.ParentId.Value);
            if (parent is null) return BadRequest(new { message = "Parent category not found" });
            if (parent.Section.ToLower() != normalizedSection)
                return BadRequest(new { message = "Parent section mismatch" });
            normalizedParentId = parent.Id;
        }

        var conflict = await FindConflictAsync(key, req.NameAr, req.NameEn, normalizedSection, normalizedParentId, null);
        if (conflict is not null)
            return BadRequest(new
            {
                message = "يوجد تصنيف مشابه بالفعل",
                conflict = ToConflictPayload(conflict),
                reason = GetConflictReason(conflict, key, req.NameAr, req.NameEn)
            });

        var entity = new CategoryDefinition
        {
            Id = Guid.NewGuid(),
            Key = key,
            NameAr = req.NameAr.Trim(),
            NameEn = req.NameEn?.Trim(),
            DescriptionAr = req.DescriptionAr?.Trim(),
            DescriptionEn = req.DescriptionEn?.Trim(),
            ImageUrl = req.ImageUrl?.Trim(),
            Section = normalizedSection,
            ParentId = normalizedParentId,
            HasDetailSections = normalizedParentId == null && req.HasDetailSections,
            SortOrder = req.SortOrder,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
        await AdminActivityWriter.LogAsync(_db, User, "create", normalizedSection == "problem" ? "problem_category" : "category", entity.Id.ToString(), $"إضافة تصنيف: {entity.NameAr}", entity.ParentId == null ? "تصنيف رئيسي" : "تصنيف دقيق", new { entity.Key, entity.NameAr, entity.Section, entity.ParentId, entity.HasDetailSections }, HttpContext.RequestAborted);
        return Ok(new { entity.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveCategoryRequest req)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        var key = Slugify(req.Key);
        if (string.IsNullOrWhiteSpace(key)) key = entity.Key;

        var normalizedSection = NormalizeSection(req.Section);
        Guid? normalizedParentId = null;
        if (req.ParentId.HasValue)
        {
            if (req.ParentId.Value == id) return BadRequest(new { message = "Category cannot be its own parent" });
            var parent = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.ParentId.Value);
            if (parent is null) return BadRequest(new { message = "Parent category not found" });
            if (parent.Section.ToLower() != normalizedSection)
                return BadRequest(new { message = "Parent section mismatch" });
            normalizedParentId = parent.Id;
        }

        var conflict = await FindConflictAsync(key, req.NameAr, req.NameEn, normalizedSection, normalizedParentId, id);
        if (conflict is not null)
            return BadRequest(new
            {
                message = "يوجد تصنيف مشابه بالفعل",
                conflict = ToConflictPayload(conflict),
                reason = GetConflictReason(conflict, key, req.NameAr, req.NameEn)
            });

        entity.Key = key;
        entity.NameAr = string.IsNullOrWhiteSpace(req.NameAr) ? entity.NameAr : req.NameAr.Trim();
        entity.NameEn = req.NameEn?.Trim();
        entity.DescriptionAr = req.DescriptionAr?.Trim();
        entity.DescriptionEn = req.DescriptionEn?.Trim();
        entity.ImageUrl = req.ImageUrl?.Trim();
        entity.Section = normalizedSection;
        entity.ParentId = normalizedParentId;
        entity.HasDetailSections = normalizedParentId == null && req.HasDetailSections;
        entity.SortOrder = req.SortOrder;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        if (!entity.HasDetailSections)
        {
            var children = await _db.Categories.Where(x => x.ParentId == entity.Id).ToListAsync();
            foreach (var child in children)
                child.IsActive = false;
        }

        await _db.SaveChangesAsync();
        await AdminActivityWriter.LogAsync(_db, User, "update", normalizedSection == "problem" ? "problem_category" : "category", entity.Id.ToString(), $"تعديل تصنيف: {entity.NameAr}", entity.ParentId == null ? "تصنيف رئيسي" : "تصنيف دقيق", new { entity.Key, entity.NameAr, entity.Section, entity.ParentId, entity.HasDetailSections, entity.IsActive }, HttpContext.RequestAborted);
        return Ok(new { entity.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        var hasChildren = await _db.Categories.AnyAsync(x => x.ParentId == id);
        if (hasChildren)
            return BadRequest(new { message = "Delete child sections first" });

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync();
        await AdminActivityWriter.LogAsync(_db, User, "delete", entity.Section == "problem" ? "problem_category" : "category", entity.Id.ToString(), $"حذف تصنيف: {entity.NameAr}", entity.ParentId == null ? "تصنيف رئيسي" : "تصنيف دقيق", new { entity.Key, entity.NameAr, entity.Section, entity.ParentId }, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPost("upload")]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    public async Task<ActionResult<object>> Upload([FromForm] IFormFile? file)
    {
        if ((file is null || file.Length == 0) && Request.HasFormContentType && Request.Form.Files.Count > 0)
            file = Request.Form.Files[0];

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required" });

        var id = Guid.NewGuid();
        var optimized = await ImageOptimizer.OptimizeImageToWebpAsync(file, HttpContext.RequestAborted);
        await using var stream = optimized.Stream;
        var key = $"uploads/categories/{id}{optimized.Extension}";
        var stored = await _storage.UploadAsync(stream, key, optimized.ContentType, HttpContext.RequestAborted);

        return Ok(new { url = stored.Url, key = stored.Key, optimized = optimized.Optimized });
    }


    private async Task<CategoryDefinition?> FindConflictAsync(string key, string? nameAr, string? nameEn, string section, Guid? parentId, Guid? currentId)
    {
        var normalizedKey = Slugify(key);
        var normalizedNameAr = NormalizeText(nameAr);
        var normalizedNameEn = NormalizeText(nameEn);

        return await _db.Categories.AsNoTracking()
            .Where(x => x.Section.ToLower() == section && x.ParentId == parentId && (!currentId.HasValue || x.Id != currentId.Value))
            .FirstOrDefaultAsync(x =>
                x.Key.ToLower() == normalizedKey ||
                x.NameAr.ToLower() == normalizedNameAr ||
                (!string.IsNullOrWhiteSpace(normalizedNameEn) && (x.NameEn ?? string.Empty).ToLower() == normalizedNameEn));
    }

    private static object ToConflictPayload(CategoryDefinition item) => new
    {
        item.Id,
        item.Key,
        item.NameAr,
        item.NameEn,
        item.ParentId,
        item.HasDetailSections,
        item.SortOrder,
        item.IsActive
    };

    private static string GetConflictReason(CategoryDefinition item, string key, string? nameAr, string? nameEn)
    {
        if (item.Key.Equals(Slugify(key), StringComparison.OrdinalIgnoreCase)) return "نفس المفتاح";
        if (NormalizeText(item.NameAr) == NormalizeText(nameAr)) return "نفس الاسم العربي";
        if (!string.IsNullOrWhiteSpace(nameEn) && NormalizeText(item.NameEn) == NormalizeText(nameEn)) return "نفس الاسم الإنكليزي";
        return "تشابه في بيانات التصنيف";
    }

    private static string NormalizeText(string? value)
    {
        var s = (value ?? string.Empty).Trim().ToLowerInvariant();
        s = s.Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا').Replace('ى', 'ي').Replace('ة', 'ه');
        s = Regex.Replace(s, "\\s+", " ");
        return s;
    }

    private static string NormalizeSection(string? value)
    {
        var v = (value ?? string.Empty).Trim().ToLowerInvariant();
        return v == "problem" ? "problem" : "regular";
    }

    private static string Slugify(string? value)
    {
        var s = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = Regex.Replace(s, "[^a-z0-9\\s_-]", string.Empty);
        s = Regex.Replace(s, "[\\s_]+", "-").Trim('-');
        return s;
    }
}
