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
    public record RegisterRequest(string FullName, string Phone, string Email, string Password, string? ReferralCode);
    public record LoginRequest(string Email, string Password, string? Identifier = null);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if ((string.IsNullOrWhiteSpace(req.Email) && string.IsNullOrWhiteSpace(req.Phone)) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Email or phone and password are required." });

        var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
        var phone = NormalizePhone(req.Phone);

        if (!string.IsNullOrWhiteSpace(email) && await db.Users.AnyAsync(u => u.Email.ToLower() == email))
            return BadRequest(new { message = "Email already exists." });
        if (!string.IsNullOrWhiteSpace(phone) && await db.Users.AnyAsync(u => u.Phone == phone))
            return BadRequest(new { message = "Phone already exists." });

        User? referrer = null;
        var referralCode = (req.ReferralCode ?? string.Empty).Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(referralCode))
            referrer = await db.Users.FirstOrDefaultAsync(u => u.ReferralCode == referralCode);

        // Optional: set one email as Admin via env/config (ADMIN_EMAIL or Admin:Email)
        var adminEmail = (cfg["Admin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "")
            .Trim().ToLowerInvariant();
        var role = (!string.IsNullOrWhiteSpace(adminEmail) && adminEmail == email) ? "Admin" : "User";

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = req.FullName?.Trim() ?? "",
            Phone = phone,
            Email = email,
            ReferralCode = await GenerateReferralCodeAsync(db),
            ReferredByUserId = referrer?.Id,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.PointsWallets.Add(new Ecommerce.Api.Domain.Entities.PointsWallet { UserId = user.Id });
        if (referrer != null)
        {
            db.Referrals.Add(new Ecommerce.Api.Domain.Entities.Referral
            {
                ReferrerUserId = referrer.Id,
                ReferredUserId = user.Id,
                ReferralCode = referralCode
            });
            db.UserGifts.Add(new Ecommerce.Api.Domain.Entities.UserGift
            {
                UserId = referrer.Id,
                GiftType = "Notice",
                Title = "تسجيل عبر رابطك",
                Message = $"حساب جديد سجل باستخدام كود مشاركتك: {user.FullName}"
            });
        }
        await db.SaveChangesAsync();

        var token = CreateJwt(user);
        return Ok(new { token, user = new { user.Id, user.FullName, user.Phone, user.Email, user.Role, user.ReferralCode } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        try
        {
            var identifier = (req.Identifier ?? req.Email ?? string.Empty).Trim();
            var normalizedEmail = identifier.ToLowerInvariant();
            var normalizedPhone = NormalizePhone(identifier);
            var user = await db.Users.FirstOrDefaultAsync(u =>
                (!string.IsNullOrWhiteSpace(u.Email) && u.Email.ToLower() == normalizedEmail) ||
                (!string.IsNullOrWhiteSpace(u.Phone) && u.Phone == normalizedPhone));
            if (user is null) return Unauthorized(new { message = "Invalid credentials." });

            if (!BCrypt.Net.BCrypt.Verify(req.Password ?? "", user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials." });

            // Optional: promote to Admin if ADMIN_EMAIL matches
            var adminEmail = (cfg["Admin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "")
                .Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(adminEmail) && adminEmail == (user.Email ?? string.Empty).ToLower() && user.Role != "Admin")
            {
                user.Role = "Admin";
                await db.SaveChangesAsync();
            }

            var token = CreateJwt(user);
            return Ok(new { token, user = new { user.Id, user.FullName, user.Phone, user.Email, user.Role, user.ReferralCode } });
        }
        catch (Exception ex)
        {
            return Problem(title: "Login failed", detail: ex.Message, statusCode: 500);
        }
    }

    private static string NormalizePhone(string? phone)
    {
        var p = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (p.StartsWith("9640")) p = "964" + p[4..];
        if (p.StartsWith("0")) p = "964" + p[1..];
        return p;
    }

    private static async Task<string> GenerateReferralCodeAsync(AppDbContext db)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = "DSB" + new string(Enumerable.Range(0, 6).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
            if (!await db.Users.AnyAsync(u => u.ReferralCode == code)) return code;
        }
        return "DSB" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
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
