using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServiceRequestsController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/servicerequests
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequestRequest req)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var service = await _db.Services
            .Include(s => s.Requirements)
            .Include(s => s.Packages)
            .FirstOrDefaultAsync(s => s.Id == req.ServiceId && s.IsPublished);

        if (service == null) return NotFound("Service not found");

        var package = service.Packages.FirstOrDefault(p => p.Id == req.PackageId);
        if (package == null) return BadRequest("Invalid package");

        // تحقق من الأسئلة الإلزامية
        var requiredIds = service.Requirements.Where(r => r.IsRequired).Select(r => r.Id).ToHashSet();
        var answeredIds = (req.Answers ?? []).Select(a => a.RequirementId).ToHashSet();

        foreach (var rid in requiredIds)
            if (!answeredIds.Contains(rid))
                return BadRequest("Missing required requirement answers");

        var sr = new ServiceRequest
        {
            UserId = userId,
            ServiceId = service.Id,
            PackageId = package.Id,
            Notes = req.Notes?.Trim() ?? "",
            Status = "PendingPayment"
        };

        foreach (var a in req.Answers ?? [])
        {
            sr.Answers.Add(new ServiceRequestAnswer
            {
                RequirementId = a.RequirementId,
                Answer = (a.Answer ?? "").Trim()
            });
        }

        _db.ServiceRequests.Add(sr);
        await _db.SaveChangesAsync();

        return Ok(new { sr.Id, sr.Status });
    }

    // GET /api/servicerequests/my
    [HttpGet("my")]
    public async Task<IActionResult> MyRequests()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var items = await _db.ServiceRequests.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.CreatedAt,
                Service = new { x.ServiceId, x.Service.Title },
                Package = new { x.PackageId, x.Package.Name, x.Package.PriceUsd, x.Package.DeliveryDays }
            })
            .ToListAsync();

        return Ok(items);
    }
}

public class CreateServiceRequestRequest
{
    public Guid ServiceId { get; set; }
    public Guid PackageId { get; set; }
    public string? Notes { get; set; }
    public List<ServiceRequestAnswerDto>? Answers { get; set; }
}

public class ServiceRequestAnswerDto
{
    public Guid RequirementId { get; set; }
    public string? Answer { get; set; }
}
