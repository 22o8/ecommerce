// Ecommerce.Api/Controllers/AdminOrdersController.cs
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminOrdersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _db.Orders.AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.Status,
                o.TotalUsd,
                o.CreatedAt,
                User = new { o.UserId, o.User.FullName, o.User.Email },
                Items = o.Items.Select(i => new
                {
                    i.ItemType,
                    i.Quantity,
                    i.UnitPriceUsd,
                    i.LineTotalUsd,
                    i.ProductId,
                    i.ServiceId,
                    i.PackageId,
                    i.ServiceRequestId
                }).ToList(),
                Payments = o.Payments.Select(p => new { p.Id, p.Provider, p.Status, p.AmountUsd }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }
}
