using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    // Catalog ثابت حتى ما نعتمد على جدول Brands (ويصير 500 إذا ما انعملت migrations).
    private static readonly string[] Brands = new[]
    {
        "Anua",
        "APRILSKIN",
        "VT (VT Global)",
        "Skinfood",
        "Medicube",
        "Numbuzin",
        "K-SECRET",
        "Equal Berry",
        "SKIN1004",
        "Beauty of Joseon",
        "JMsolution",
        "Tenzero",
        "Dr.Ceuracle",
        "Rejuran",
        "Celimax",
        "Medipeel",
        "Biodance",
        "Dr.CPU",
        "Anua KR",
    };

    [HttpGet]
    public IActionResult Get()
    {
        // يرجّعها بشكل بسيط { id, name } حتى تكون جاهزة للفلترة بالفرونت.
        var data = Brands.Select((name, i) => new
        {
            id = i + 1,
            name,
            key = name
                .ToLowerInvariant()
                .Replace("(", "")
                .Replace(")", "")
                .Replace(" ", "-")
        });

        return Ok(data);
    }
}
