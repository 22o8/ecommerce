using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/services")]
[Authorize(Roles = "Admin")]
public class AdminServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminServicesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _db.Services.AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id, s.Title, s.Slug, s.Description, s.IsPublished, s.CreatedAt,
                Packages = s.Packages.Select(p => new { p.Id, p.Name, p.PriceIqd,
                        p.PriceUsd, p.DeliveryDays, p.Features }).ToList(),
                Requirements = s.Requirements.OrderBy(r => r.Order).Select(r => new { r.Id, r.Question, r.IsRequired, r.Order }).ToList()
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title)) return BadRequest("Title is required");

        var slug = (req.Slug ?? "").Trim();
        if (string.IsNullOrWhiteSpace(slug))
            slug = req.Title.Trim().ToLower().Replace(" ", "-");

        // توليد slug لو موجود
        var baseSlug = slug;
        var i = 2;
        while (await _db.Services.AnyAsync(x => x.Slug == slug))
        {
            slug = $"{baseSlug}-{i}";
            i++;
        }

        var service = new Service
        {
            Title = req.Title.Trim(),
            Slug = slug,
            Description = req.Description?.Trim() ?? "",
            IsPublished = req.IsPublished
        };

        // باقات
        foreach (var p in req.Packages ?? [])
        {
            service.Packages.Add(new ServicePackage
            {
                Name = (p.Name ?? "Basic").Trim(),
                PriceIqd = (p.PriceIqd > 0 ? p.PriceIqd : p.PriceUsd),
                PriceUsd = p.PriceUsd,
                DeliveryDays = p.DeliveryDays <= 0 ? 3 : p.DeliveryDays,
                Features = p.Features?.Trim() ?? ""
            });
        }

        // أسئلة
        foreach (var r in req.Requirements ?? [])
        {
            service.Requirements.Add(new ServiceRequirementTemplate
            {
                Question = (r.Question ?? "").Trim(),
                IsRequired = r.IsRequired,
                Order = r.Order <= 0 ? 1 : r.Order
            });
        }

        _db.Services.Add(service);
        await _db.SaveChangesAsync();

        return Ok(new { service.Id, service.Title, service.Slug });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest req)
    {
        var service = await _db.Services
            .Include(s => s.Packages)
            .Include(s => s.Requirements)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null) return NotFound();

        if (req.Title != null && !string.IsNullOrWhiteSpace(req.Title))
            service.Title = req.Title.Trim();

        if (req.Description != null)
            service.Description = req.Description.Trim();

        if (req.IsPublished.HasValue)
            service.IsPublished = req.IsPublished.Value;

        // تحديث slug (اختياري)
        if (req.Slug != null)
        {
            var slug = req.Slug.Trim();
            if (string.IsNullOrWhiteSpace(slug)) return BadRequest("Slug cannot be empty");
            if (await _db.Services.AnyAsync(x => x.Slug == slug && x.Id != id))
                return Conflict("Slug already exists");

            service.Slug = slug;
        }

        // استبدال الباقات/الأسئلة إذا انرسلت
        if (req.Packages != null)
        {
            service.Packages.Clear();
            foreach (var p in req.Packages)
            {
                service.Packages.Add(new ServicePackage
                {
                    Name = (p.Name ?? "Basic").Trim(),
                    PriceIqd = (p.PriceIqd > 0 ? p.PriceIqd : p.PriceUsd),
                    PriceUsd = p.PriceUsd,
                    DeliveryDays = p.DeliveryDays <= 0 ? 3 : p.DeliveryDays,
                    Features = p.Features?.Trim() ?? ""
                });
            }
        }

        if (req.Requirements != null)
        {
            service.Requirements.Clear();
            foreach (var r in req.Requirements)
            {
                service.Requirements.Add(new ServiceRequirementTemplate
                {
                    Question = (r.Question ?? "").Trim(),
                    IsRequired = r.IsRequired,
                    Order = r.Order <= 0 ? 1 : r.Order
                });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { service.Id, service.Title, service.Slug, service.IsPublished });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return NotFound();

        _db.Services.Remove(service);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateServiceRequest
{
    public string Title { get; set; } = "";
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public bool IsPublished { get; set; } = true;

    public List<ServicePackageDto>? Packages { get; set; }
    public List<ServiceRequirementDto>? Requirements { get; set; }
}

public class UpdateServiceRequest
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public bool? IsPublished { get; set; }

    public List<ServicePackageDto>? Packages { get; set; }
    public List<ServiceRequirementDto>? Requirements { get; set; }
}

public class ServicePackageDto
{
    public string Name { get; set; } = "Basic";
    public decimal PriceIqd { get; set; }

        public decimal PriceUsd { get; set; }
    public int DeliveryDays { get; set; } = 3;
    public string? Features { get; set; }
}

public class ServiceRequirementDto
{
    public string Question { get; set; } = "";
    public bool IsRequired { get; set; } = true;
    public int Order { get; set; } = 1;
}
