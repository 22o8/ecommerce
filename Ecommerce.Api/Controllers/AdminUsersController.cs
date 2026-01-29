using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminUsersController(AppDbContext db)
    {
        _db = db;
    }

    public sealed record UserListItem(
        Guid Id,
        string Email,
        string? Name,
        string Role,
        bool IsActive,
        DateTime CreatedAt
    );

    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Total,
        int Page,
        int PageSize
    );

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserListItem>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(u => u.Email.Contains(s) || (u.Name != null && u.Name.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim().ToLower();
            query = query.Where(u => u.Role.ToLower() == r);
        }

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItem(
                u.Id,
                u.Email,
                u.Name,
                u.Role,
                u.IsActive,
                u.CreatedAt
            ))
            .ToListAsync();

        return Ok(new PagedResult<UserListItem>(items, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        // لا نرجّع كلمة المرور
        user.PasswordHash = string.Empty;
        return Ok(user);
    }

    public sealed record CreateUserRequest(
        string Email,
        string Password,
        string? Name,
        string? Role,
        bool? IsActive
    );

    [HttpPost]
    public async Task<ActionResult<UserListItem>> Create([FromBody] CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and password are required");

        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email))
            return Conflict("Email already exists");

        var role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role.Trim();
        role = role.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = req.Name?.Trim(),
            Role = role,
            IsActive = req.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new UserListItem(user.Id, user.Email, user.Name, user.Role, user.IsActive, user.CreatedAt));
    }

    public sealed record UpdateUserRequest(
        string? Email,
        string? Name,
        string? Role,
        bool? IsActive,
        string? NewPassword
    );

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserListItem>> Update(Guid id, [FromBody] UpdateUserRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var exists = await _db.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == email);
            if (exists) return Conflict("Email already exists");
            user.Email = email;
        }

        if (req.Name is not null)
            user.Name = string.IsNullOrWhiteSpace(req.Name) ? null : req.Name.Trim();

        if (!string.IsNullOrWhiteSpace(req.Role))
        {
            var role = req.Role.Trim();
            user.Role = role.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";
        }

        if (req.IsActive.HasValue)
            user.IsActive = req.IsActive.Value;

        if (!string.IsNullOrWhiteSpace(req.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        await _db.SaveChangesAsync();

        return Ok(new UserListItem(user.Id, user.Email, user.Name, user.Role, user.IsActive, user.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
