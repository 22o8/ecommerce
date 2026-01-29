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

    // =========================
    // Helpers: support shadow/optional properties
    // =========================
    private bool HasProp(string propName)
        => _db.Model.FindEntityType(typeof(User))?.FindProperty(propName) is not null;

    private static string? SafeLike(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static string NormalizeRole(string? role)
        => string.Equals(role?.Trim(), "admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

    private string? GetNameQuery(User u)
        => HasProp("Name") ? EF.Property<string?>(u, "Name") : null;

    private bool GetIsActiveQuery(User u)
        => HasProp("IsActive") ? EF.Property<bool>(u, "IsActive") : true;

    private void SetOptionalProps(User user, string? name, bool? isActive)
    {
        // Name (optional)
        if (HasProp("Name"))
        {
            _db.Entry(user).Property("Name").CurrentValue = SafeLike(name);
        }

        // IsActive (optional)
        if (HasProp("IsActive") && isActive.HasValue)
        {
            _db.Entry(user).Property("IsActive").CurrentValue = isActive.Value;
        }
    }

    private UserListItem ToListItem(User u)
    {
        var name = HasProp("Name") ? EF.Property<string?>(u, "Name") : null;
        var isActive = HasProp("IsActive") ? EF.Property<bool>(u, "IsActive") : true;

        return new UserListItem(
            u.Id,
            u.Email,
            name,
            u.Role,
            isActive,
            u.CreatedAt
        );
    }

    // =========================
    // List
    // =========================
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

            if (HasProp("Name"))
            {
                query = query.Where(u =>
                    u.Email.Contains(s) ||
                    (EF.Property<string?>(u, "Name") != null && EF.Property<string?>(u, "Name")!.Contains(s))
                );
            }
            else
            {
                query = query.Where(u => u.Email.Contains(s));
            }
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim().ToLower();
            query = query.Where(u => u.Role.ToLower() == r);
        }

        if (isActive.HasValue && HasProp("IsActive"))
        {
            query = query.Where(u => EF.Property<bool>(u, "IsActive") == isActive.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItem(
                u.Id,
                u.Email,
                // Name (optional)
                HasProp("Name") ? EF.Property<string?>(u, "Name") : null,
                u.Role,
                // IsActive (optional)
                HasProp("IsActive") ? EF.Property<bool>(u, "IsActive") : true,
                u.CreatedAt
            ))
            .ToListAsync();

        return Ok(new PagedResult<UserListItem>(items, total, page, pageSize));
    }

    // =========================
    // Get single
    // =========================
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        // لا نرجّع كلمة المرور
        user.PasswordHash = string.Empty;
        return Ok(user);
    }

    // =========================
    // Create
    // =========================
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

        var email = NormalizeEmail(req.Email);

        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == email);
        if (exists) return Conflict("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Role = NormalizeRole(req.Role),
            CreatedAt = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        // set optional fields safely (even if not in entity)
        SetOptionalProps(user, req.Name, req.IsActive ?? true);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // read back optional props via Entry (ensures values present even if shadow)
        var name = HasProp("Name") ? _db.Entry(user).Property("Name").CurrentValue as string : null;
        var active = HasProp("IsActive") ? (bool)(_db.Entry(user).Property("IsActive").CurrentValue ?? true) : true;

        return Ok(new UserListItem(user.Id, user.Email, name, user.Role, active, user.CreatedAt));
    }

    // =========================
    // Update
    // =========================
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
            var email = NormalizeEmail(req.Email);

            var emailExists = await _db.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == email);
            if (emailExists) return Conflict("Email already exists");

            user.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(req.Role))
        {
            user.Role = NormalizeRole(req.Role);
        }

        if (!string.IsNullOrWhiteSpace(req.NewPassword))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        }

        // optional fields
        if (req.Name is not null || req.IsActive.HasValue)
        {
            SetOptionalProps(user, req.Name, req.IsActive);
        }

        await _db.SaveChangesAsync();

        // build response (optional props)
        var name = HasProp("Name") ? _db.Entry(user).Property("Name").CurrentValue as string : null;
        var active = HasProp("IsActive") ? (bool)(_db.Entry(user).Property("IsActive").CurrentValue ?? true) : true;

        return Ok(new UserListItem(user.Id, user.Email, name, user.Role, active, user.CreatedAt));
    }

    // =========================
    // Delete
    // =========================
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
