using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecommerce.Api.Infrastructure;

public static class BrandCatalog
{
    // ✅ قائمة البراندات المعتمدة (تقدر توسّعها لاحقاً)
    public static readonly IReadOnlyList<string> Allowed = new List<string>
    {
        "Unspecified",
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

    public static bool IsAllowed(string? brand)
    {
        var b = Normalize(brand);
        return Allowed.Contains(b);
    }

    public static string Normalize(string? brand)
    {
        var b = (brand ?? "").Trim();
        if (string.IsNullOrWhiteSpace(b)) return "Unspecified";
        // توحيد بسيط: خلّيها نفس الحالة الموجودة بالقائمة إذا ممكن
        var match = Allowed.FirstOrDefault(x => string.Equals(x, b, StringComparison.OrdinalIgnoreCase));
        return match ?? b;
    }
}
