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
        // ✅ مهم: تجنب Include(User) لأنه يقرأ كل أعمدة Users وقد يسبب 500 إذا DB غير محدثة (مثل Phone)
        try
        {
            var orders = await _db.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.TotalUsd,
                    o.CreatedAt,

                    User = new
                    {
                        o.UserId,
                        FullName = o.User.FullName,
                        Email = o.User.Email
                    },

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

                    Payments = o.Payments.Select(p => new
                    {
                        p.Id,
                        p.Provider,
                        p.Status,
                        p.AmountUsd
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load orders.", detail = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound(new { message = "Order not found." });

        _db.OrderItems.RemoveRange(order.Items);
        _db.Payments.RemoveRange(order.Payments);
        _db.Orders.Remove(order);

        await _db.SaveChangesAsync();
        return Ok(new { message = "Order deleted.", id });
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAll()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .ToListAsync();

        _db.OrderItems.RemoveRange(orders.SelectMany(o => o.Items));
        _db.Payments.RemoveRange(orders.SelectMany(o => o.Payments));
        _db.Orders.RemoveRange(orders);

        await _db.SaveChangesAsync();
        return Ok(new { message = "All orders deleted.", count = orders.Count });
    }
}
