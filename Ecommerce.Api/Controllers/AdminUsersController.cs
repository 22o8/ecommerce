using Ecommerce.Api.Infrastructure.Data;
using Ecommerce.Api.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        string Phone,
        string? Name,
        string? FullName,
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

    private bool HasProp(string propName)
        => _db.Model.FindEntityType(typeof(User))?.FindProperty(propName) is not null;

    private static string? CleanOptional(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string CleanText(string? s)
        => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();

    private static string NormalizeEmail(string? email)
        => string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim().ToLowerInvariant();

    private static string NormalizePhone(string? phone)
    {
        var p = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (p.StartsWith("9640")) p = "964" + p[4..];
        if (p.StartsWith("00964")) p = "964" + p[5..];
        if (p.StartsWith("+964")) p = "964" + p[4..];
        return p;
    }

    private static string NormalizeRole(string? role)
        => string.Equals(role?.Trim(), "admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

    private bool GetIsActive(User u)
        => HasProp("IsActive") ? EF.Property<bool>(u, "IsActive") : true;

    private void SetOptionalProps(User user, bool? isActive)
    {
        if (HasProp("IsActive") && isActive.HasValue)
            _db.Entry(user).Property("IsActive").CurrentValue = isActive.Value;
    }

    private UserListItem ToListItem(User u)
    {
        var active = HasProp("IsActive") ? (bool)(_db.Entry(u).Property("IsActive").CurrentValue ?? true) : true;
        return new UserListItem(
            u.Id,
            u.Email ?? string.Empty,
            u.Phone ?? string.Empty,
            u.FullName,
            u.FullName,
            u.Role,
            active,
            u.CreatedAt
        );
    }

    private async Task EnsureUsersSchemaAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""FullName"" character varying(220) NOT NULL DEFAULT '';");
        await _db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""Phone"" character varying(40) NOT NULL DEFAULT '';");
        await _db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""ReferralCode"" character varying(24) NOT NULL DEFAULT '';");
        await _db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""ReferredByUserId"" uuid NULL;");

        await _db.Database.ExecuteSqlRawAsync(@"DROP INDEX IF EXISTS ""IX_Users_Email"";");
        await _db.Database.ExecuteSqlRawAsync(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"") WHERE ""Email"" IS NOT NULL AND btrim(""Email"") <> '';");
        await _db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Users_Phone"" ON ""Users"" (""Phone"");");
        await _db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Users_ReferralCode"" ON ""Users"" (""ReferralCode"");");
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserListItem>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        await EnsureUsersSchemaAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(s)) ||
                (u.Phone != null && u.Phone.Contains(s)) ||
                (u.FullName != null && u.FullName.Contains(s))
            );
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim().ToLower();
            query = query.Where(u => u.Role.ToLower() == r);
        }

        if (isActive.HasValue && HasProp("IsActive"))
            query = query.Where(u => EF.Property<bool>(u, "IsActive") == isActive.Value);

        var total = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = users.Select(u => new UserListItem(
            u.Id,
            u.Email ?? string.Empty,
            u.Phone ?? string.Empty,
            u.FullName,
            u.FullName,
            u.Role,
            GetIsActive(u),
            u.CreatedAt
        )).ToList();

        return Ok(new PagedResult<UserListItem>(items, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> Get(Guid id)
    {
        await EnsureUsersSchemaAsync();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        return Ok(new
        {
            user.Id,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Name = user.FullName,
            FullName = user.FullName,
            user.Role,
            IsActive = true,
            user.CreatedAt,
            user.ReferralCode
        });
    }

    public sealed record CreateUserRequest(
        string? Email,
        string? Phone,
        string Password,
        string? Name,
        string? FullName,
        string? Role,
        bool? IsActive
    );

    [HttpPost]
    public async Task<ActionResult<UserListItem>> Create([FromBody] CreateUserRequest req)
    {
        await EnsureUsersSchemaAsync();
        var email = NormalizeEmail(req.Email);
        var phone = NormalizePhone(req.Phone);
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            return BadRequest(new { message = "يجب إدخال إيميل أو رقم هاتف." });
        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "كلمة المرور مطلوبة." });

        if (!string.IsNullOrWhiteSpace(email) && await _db.Users.AnyAsync(u => u.Email.ToLower() == email))
            return Conflict(new { message = "هذا الإيميل مسجل مسبقاً." });
        if (!string.IsNullOrWhiteSpace(phone) && await _db.Users.AnyAsync(u => u.Phone == phone))
            return Conflict(new { message = "رقم الهاتف مسجل مسبقاً." });

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = CleanText(req.FullName ?? req.Name),
            Email = email,
            Phone = phone,
            Role = NormalizeRole(req.Role),
            CreatedAt = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            ReferralCode = "DSB" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()
        };

        SetOptionalProps(user, req.IsActive ?? true);
        _db.Users.Add(user);

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { message = "الإيميل أو رقم الهاتف موجود مسبقاً." });
        }

        return Ok(ToListItem(user));
    }

    public sealed record UpdateUserRequest(
        string? Email,
        string? Phone,
        string? Name,
        string? FullName,
        string? Role,
        bool? IsActive,
        string? NewPassword
    );

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserListItem>> Update(Guid id, [FromBody] UpdateUserRequest req)
    {
        await EnsureUsersSchemaAsync();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        var email = NormalizeEmail(req.Email);
        var phone = NormalizePhone(req.Phone);

        if (!string.IsNullOrWhiteSpace(email) && await _db.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == email))
            return Conflict(new { message = "هذا الإيميل مسجل مسبقاً." });
        if (!string.IsNullOrWhiteSpace(phone) && await _db.Users.AnyAsync(u => u.Id != id && u.Phone == phone))
            return Conflict(new { message = "رقم الهاتف مسجل مسبقاً." });

        // مهم: إذا المستخدم بدون إيميل، لا نعوضه بإيميل الأدمن أبداً.
        user.Email = email;
        user.Phone = phone;
        user.FullName = CleanText(req.FullName ?? req.Name);

        if (!string.IsNullOrWhiteSpace(req.Role)) user.Role = NormalizeRole(req.Role);
        if (!string.IsNullOrWhiteSpace(req.NewPassword)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        SetOptionalProps(user, req.IsActive);

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { message = "الإيميل أو رقم الهاتف موجود مسبقاً." });
        }

        return Ok(ToListItem(user));
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

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pg && pg.SqlState == "23505") return true;
        var text = ex.ToString();
        return text.Contains("23505") || text.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
    }
}
