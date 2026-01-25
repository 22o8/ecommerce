using Ecommerce.Api.Domain.Entities;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, IConfiguration cfg) : ControllerBase
{
    public record RegisterRequest(string FullName, string Phone, string Email, string Password);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Email and password are required." });

        var email = req.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email.ToLower() == email))
            return BadRequest(new { message = "Email already exists." });

        // Optional: set one email as Admin via env/config (ADMIN_EMAIL or Admin:Email)
        var adminEmail = (cfg["Admin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "")
            .Trim().ToLowerInvariant();
        var role = (!string.IsNullOrWhiteSpace(adminEmail) && adminEmail == email) ? "Admin" : "User";

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = req.FullName?.Trim() ?? "",
            Phone = req.Phone?.Trim() ?? "",
            Email = email,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = CreateJwt(user);
        return Ok(new { token, user = new { user.Id, user.FullName, user.Phone, user.Email, user.Role } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        try
        {
            var email = req.Email?.Trim().ToLowerInvariant() ?? "";
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user is null) return Unauthorized(new { message = "Invalid credentials." });

            if (!BCrypt.Net.BCrypt.Verify(req.Password ?? "", user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials." });

            // Optional: promote to Admin if ADMIN_EMAIL matches
            var adminEmail = (cfg["Admin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "")
                .Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(adminEmail) && adminEmail == email && user.Role != "Admin")
            {
                user.Role = "Admin";
                await db.SaveChangesAsync();
            }

            var token = CreateJwt(user);
            return Ok(new { token, user = new { user.Id, user.FullName, user.Phone, user.Email, user.Role } });
        }
        catch (Exception ex)
        {
            return Problem(title: "Login failed", detail: ex.Message, statusCode: 500);
        }
    }

    private string CreateJwt(User user)
    {
        // Works on Render/containers: read from env/config, fallback for demo.
        var key =
            cfg["Jwt:Key"] ??
            Environment.GetEnvironmentVariable("JWT_SECRET") ??
            Environment.GetEnvironmentVariable("Jwt__Key") ??
            "DEV_ONLY_CHANGE_ME";

        var issuer =
            cfg["Jwt:Issuer"] ??
            Environment.GetEnvironmentVariable("JWT_ISSUER") ??
            Environment.GetEnvironmentVariable("Jwt__Issuer") ??
            "ecommerce-api";

        var audience =
            cfg["Jwt:Audience"] ??
            Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
            Environment.GetEnvironmentVariable("Jwt__Audience") ??
            "ecommerce-web";

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role ?? "User"),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
