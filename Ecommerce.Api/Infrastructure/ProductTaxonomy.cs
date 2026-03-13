using System.Text.RegularExpressions;

namespace Ecommerce.Api.Infrastructure;

public static class ProductTaxonomy
{
    public sealed record TaxonomyOption(string Key, string[] Aliases);

    public static readonly string[] Categories =
    {
        "serum",
        "moisturizer",
        "sunscreen",
        "cleanser",
        "perfume",
        "eye-care",
        "toner",
        "mask",
        "lip-care"
    };

    public static readonly Dictionary<string, string[]> CategoryAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["serum"] = new[] { "serum", "سيروم", "سيرم" },
        ["moisturizer"] = new[] { "moisturizer", "moisturiser", "cream", "face cream", "مرطب", "كريم", "كريم الوجه" },
        ["sunscreen"] = new[] { "sunscreen", "sun screen", "spf", "واقي", "واقي شمس", "sunblock" },
        ["cleanser"] = new[] { "cleanser", "face wash", "wash", "غسول", "منظف", "cleaning gel" },
        ["perfume"] = new[] { "perfume", "fragrance", "عطر", "perfume mist" },
        ["eye-care"] = new[] { "eye care", "under eye", "حول العين", "للعين", "eye" },
        ["toner"] = new[] { "toner", "تونر" },
        ["mask"] = new[] { "mask", "ماسك", "قناع" },
        ["lip-care"] = new[] { "lip", "lip balm", "lip care", "شفايف", "شفاه" },
    };

    public static readonly Dictionary<string, string[]> SubCategoryAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eye-serum"] = new[] { "eye serum", "serum eye", "serum for eyes", "سيروم العين", "سيروم للعين", "سيروم حول العين" },
        ["eye-cream"] = new[] { "eye cream", "cream for eyes", "كريم العين", "كريم للعين", "كريم حول العين" },
        ["eye-gel"] = new[] { "eye gel", "جل العين", "جل للعين", "جل حول العين" },
        ["face-serum"] = new[] { "face serum", "serum", "سيروم", "سيروم الوجه" },
        ["face-cream"] = new[] { "face cream", "moisturizer", "moisturiser", "مرطب", "كريم الوجه" },
        ["night-cream"] = new[] { "night cream", "كريم ليلي" },
        ["day-cream"] = new[] { "day cream", "كريم نهاري" },
        ["gel-cleanser"] = new[] { "gel cleanser", "غسول جل", "جل غسول" },
        ["foam-cleanser"] = new[] { "foam cleanser", "غسول رغوي", "foam wash" },
        ["sun-fluid"] = new[] { "sun fluid", "fluide sunscreen", "واقي شمس فلويد" },
        ["sun-cream"] = new[] { "sun cream", "واقي شمس كريم" },
        ["lip-balm"] = new[] { "lip balm", "مرطب شفاه", "بلسم شفاه" },
    };

    public static string Normalize(string? value)
    {
        var s = (value ?? string.Empty).Trim().ToLowerInvariant();
        s = Regex.Replace(s, "[`'\"]+", string.Empty);
        s = Regex.Replace(s, @"[^a-z0-9\u0600-\u06FF]+", "-");
        s = Regex.Replace(s, @"-+", "-");
        return s.Trim('-');
    }

    public static bool IsValidCategory(string? category)
    {
        var c = Normalize(category);
        return !string.IsNullOrWhiteSpace(c) && Categories.Contains(c, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsValidSubCategory(string? subCategory)
    {
        var c = Normalize(subCategory);
        return string.IsNullOrWhiteSpace(c) || SubCategoryAliases.ContainsKey(c);
    }

    public static string DetectCategoryFromQuery(string? q)
    {
        var query = (q ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query)) return string.Empty;
        var normalized = Normalize(query).Replace('-', ' ');
        foreach (var pair in CategoryAliases)
        {
            if (pair.Value.Any(a => normalized.Contains(Normalize(a).Replace('-', ' '), StringComparison.OrdinalIgnoreCase)))
                return Normalize(pair.Key);
        }
        return string.Empty;
    }

    public static string DetectSubCategoryFromQuery(string? q)
    {
        var query = (q ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query)) return string.Empty;
        var normalized = Normalize(query).Replace('-', ' ');
        foreach (var pair in SubCategoryAliases.OrderByDescending(x => x.Key.Length))
        {
            if (pair.Value.Any(a => normalized.Contains(Normalize(a).Replace('-', ' '), StringComparison.OrdinalIgnoreCase)))
                return Normalize(pair.Key);
        }
        return string.Empty;
    }
}
