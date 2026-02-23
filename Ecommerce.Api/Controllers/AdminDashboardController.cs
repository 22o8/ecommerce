using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminDashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalOrders = await _db.Orders.CountAsync();
        var totalUsers = await _db.Users.CountAsync();

        // Revenue: sum of successful payments in IQD.
        var totalRevenueIqd = await _db.Payments
            .Where(p => p.Status == "Succeeded")
            .SumAsync(p => (decimal?)p.AmountIqd) ?? 0m;

        return Ok(new
        {
            totalOrders,
            totalUsers,
            totalRevenueIqd
        });
    }
}
