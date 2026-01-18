using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminProductsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // ============================
    // CRUD
    // ============================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.PriceUsd,
                p.IsPublished,
                imagesCount = _db.ProductImages.Count(i => i.ProductId == p.Id)
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.Description,
                x.PriceUsd,
                x.IsPublished,
                images = _db.ProductImages
                    .Where(i => i.ProductId == x.Id)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new
                    {
                        i.Id,
                        i.Url,
                        i.Alt,
                        i.SortOrder
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (p == null)
            return NotFound(new { message = "Product not found" });

        return Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertProductRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var slug = string.IsNullOrWhiteSpace(req.Slug)
            ? Slugify(req.Title)
            : req.Slug.Trim().ToLower();

        if (await _db.Products.AnyAsync(x => x.Slug == slug))
            return BadRequest(new { message = "Slug already exists" });

        var p = new Product
        {
            Id = Guid.NewGuid(),
            Title = req.Title.Trim(),
            Slug = slug,
            Description = req.Description ?? "",
            PriceUsd = req.PriceUsd,
            IsPublished = req.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(p);
        await _db.SaveChangesAsync();

        return Ok(new { p.Id });
    }

    // ============================
    // IMAGES
    // ============================

    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(Guid id)
    {
        var images = await _db.ProductImages
            .Where(x => x.ProductId == id)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id,
                x.Url,
                x.Alt,
                x.SortOrder
            })
            .ToListAsync();

        return Ok(new { items = images });
    }

    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files");

        var product = await _db.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        var root = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var dir = Path.Combine(root, "uploads", "products", id.ToString());
        Directory.CreateDirectory(dir);

        var maxSort = await _db.ProductImages
            .Where(x => x.ProductId == id)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? 0;

        var result = new List<object>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var name = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(dir, name);

            await using var fs = System.IO.File.Create(path);
            await file.CopyToAsync(fs);

            var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/products/{id}/{name}";

            var img = new ProductImage
            {
                ProductId = id,
                Url = publicUrl,
                Alt = Path.GetFileNameWithoutExtension(file.FileName),
                SortOrder = ++maxSort
            };

            _db.ProductImages.Add(img);
            result.Add(img);
        }

        await _db.SaveChangesAsync();
        return Ok(new { items = result });
    }

    // ============================
    // HELPERS
    // ============================

    private static string Slugify(string input)
    {
        return string.Join("-",
            input.ToLower()
                 .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

// ============================
// REQUESTS
// ============================

public class UpsertProductRequest
{
    [Required, MinLength(2)]
    public string Title { get; set; } = "";

    public string? Slug { get; set; }
    public string? Description { get; set; }

    [Range(0, 999999)]
    public decimal PriceUsd { get; set; }

    public bool IsPublished { get; set; }
}
