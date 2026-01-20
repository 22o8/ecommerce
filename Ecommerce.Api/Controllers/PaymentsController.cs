using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentsController(AppDbContext db)
    {
        _db = db;
    }

    // POST /api/payments/mock/confirm/{paymentId}
    // (Admin فقط) لتأكيد الدفع يدويًا أثناء التطوير
    [HttpPost("mock/confirm/{paymentId:guid}")]
    public async Task<IActionResult> ConfirmMock(Guid paymentId)
    {
        var payment = await _db.Payments
            .Include(p => p.Order)
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return NotFound("Payment not found");
        if (payment.Provider != "Mock") return BadRequest("Not a mock payment");
        if (payment.Status == "Succeeded") return Ok("Already succeeded");

        payment.Status = "Succeeded";
        payment.Order.Status = "Paid";

        // إذا كانت الطلبية خدمة: حدّث ServiceRequest
        var serviceItem = payment.Order.Items.FirstOrDefault(i => i.ItemType == "Service" && i.ServiceRequestId != null);
        if (serviceItem?.ServiceRequestId != null)
        {
            var sr = await _db.ServiceRequests.FirstOrDefaultAsync(x => x.Id == serviceItem.ServiceRequestId.Value);
            if (sr != null)
                sr.Status = "Paid"; // أو مباشرة InProgress حسب رغبتك
        }

        await _db.SaveChangesAsync();
        return Ok(new
        {
            paymentId = payment.Id,
            paymentStatus = payment.Status,
            orderId = payment.Order.Id,
            orderStatus = payment.Order.Status
        });
    }
}
