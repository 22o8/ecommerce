using Ecommerce.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var items = BrandCatalog.Allowed
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new { key = x, label = x })
            .ToList();
        return Ok(new { items });
    }
}
