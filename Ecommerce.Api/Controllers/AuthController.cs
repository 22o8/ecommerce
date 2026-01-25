// Ecommerce.Api/Controllers/AuthController.cs
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
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLower();

        if (string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest("FullName is required");

        if (string.IsNullOrWhiteSpace(req.Phone))
            return BadRequest("Phone is required");

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required");

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return BadRequest("Password must be >= 6 chars");

        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists) return Conflict("Email already exists");

        var user = new User
        {
            FullName = req.FullName.Trim(),
            Phone = req.Phone.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { user.Id, user.FullName, user.Phone, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLower();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return Unauthorized("Invalid email or password");

        var ok = BCrypt.Net.BCrypt.Verify(req.Password ?? "", user.PasswordHash);
        if (!ok) return Unauthorized("Invalid email or password");

        var token = CreateJwt(user);
        return Ok(new { token, user = new { user.Id, user.FullName, user.Email, user.Role } });
    }

    private string CreateJwt(User user)
    {
        var jwt = _config.GetSection("Jwt");

        // Render / environment friendly: allow common env var names too.
        var rawKey = jwt["Key"]
                     ?? _config["Jwt:Key"]
                     ?? _config["Jwt__Key"]
                     ?? _config["JWT_KEY"]
                     ?? _config["JWTSECRET"]
                     ?? _config["JWT_SECRET"];

        if (string.IsNullOrWhiteSpace(rawKey))
            throw new InvalidOperationException("JWT key is missing. Set Jwt:Key (or Jwt__Key) in configuration / environment variables.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("fullName", user.FullName)
        };

        var expiresMinutesRaw = jwt["ExpiresMinutes"]
                                ?? _config["Jwt:ExpiresMinutes"]
                                ?? _config["Jwt__ExpiresMinutes"]
                                ?? _config["JWT_EXPIRES_MINUTES"];
        if (!int.TryParse(expiresMinutesRaw, out var expiresMinutes)) expiresMinutes = 60;

        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var issuer = jwt["Issuer"] ?? _config["Jwt:Issuer"] ?? _config["Jwt__Issuer"] ?? "ecommerce";
        var audience = jwt["Audience"] ?? _config["Jwt:Audience"] ?? _config["Jwt__Audience"] ?? "ecommerce";

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterRequest
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
