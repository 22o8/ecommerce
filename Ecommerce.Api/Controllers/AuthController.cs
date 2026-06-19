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
    public record RegisterRequest(string? FullName, string? Phone, string? Email, string Password, string? ReferralCode);
    public record LoginRequest(string? Email, string Password, string? Identifier = null);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        await EnsureAuthSchemaAsync();

        if ((string.IsNullOrWhiteSpace(req.Email) && string.IsNullOrWhiteSpace(req.Phone)) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "يجب إدخال إيميل أو رقم هاتف مع كلمة المرور." });

        var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
        var phone = NormalizePhone(req.Phone);

        if (!string.IsNullOrWhiteSpace(email) && await db.Users.AnyAsync(u => u.Email.ToLower() == email))
            return BadRequest(new { message = "هذا الإيميل مسجل مسبقاً." });
        if (!string.IsNullOrWhiteSpace(phone) && await db.Users.AnyAsync(u => u.Phone == phone))
            return BadRequest(new { message = "رقم الهاتف مسجل مسبقاً." });

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

        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return BadRequest(new { message = "الحساب موجود مسبقاً. جرّب تسجيل الدخول أو استخدم إيميل/رقم مختلف." });
        }

        await EnsureWelcomeAdSchemaAsync();

        var wallet = await db.PointsWallets.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (wallet is null)
        {
            wallet = new Ecommerce.Api.Domain.Entities.PointsWallet { UserId = user.Id };
            db.PointsWallets.Add(wallet);
        }

        var welcomeOffer = await GetActiveWelcomeOfferAsync();
        if (welcomeOffer is not null)
        {
            if (welcomeOffer.WelcomePoints > 0)
            {
                wallet.Balance += welcomeOffer.WelcomePoints;
                wallet.LifetimeEarned += welcomeOffer.WelcomePoints;
                wallet.UpdatedAtUtc = DateTime.UtcNow;
                db.PointsTransactions.Add(new Ecommerce.Api.Domain.Entities.PointsTransaction
                {
                    Wallet = wallet,
                    UserId = user.Id,
                    Points = welcomeOffer.WelcomePoints,
                    Type = "WelcomeGift",
                    Note = "نقاط ترحيب للمستخدم الجديد"
                });
            }

            db.UserGifts.Add(new Ecommerce.Api.Domain.Entities.UserGift
            {
                UserId = user.Id,
                GiftType = string.IsNullOrWhiteSpace(welcomeOffer.WelcomeCouponCode) ? "Welcome" : "Coupon",
                Title = welcomeOffer.Title,
                Message = BuildWelcomeMessage(welcomeOffer),
                Points = welcomeOffer.WelcomePoints,
                CouponCode = string.IsNullOrWhiteSpace(welcomeOffer.WelcomeCouponCode) ? null : welcomeOffer.WelcomeCouponCode
            });
        }

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
        return Ok(new
        {
            token,
            user = new { user.Id, user.FullName, user.Phone, user.Email, user.Role, user.ReferralCode },
            welcomeOffer = welcomeOffer is null ? null : new
            {
                welcomeOffer.Id,
                welcomeOffer.Title,
                welcomeOffer.Subtitle,
                welcomeOffer.ImageUrl,
                welcomeOffer.LinkUrl,
                couponCode = welcomeOffer.WelcomeCouponCode,
                points = welcomeOffer.WelcomePoints
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        try
        {
            await EnsureAuthSchemaAsync();

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

    private async Task EnsureAuthSchemaAsync()
    {
        // دفاع إضافي: إذا قاعدة الإنتاج ما طبقت المايغريشن بعد، لا نخلي التسجيل/الدخول يطيح 500.
        // هذه الأوامر آمنة لأنها تستخدم IF NOT EXISTS.
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""FullName"" character varying(220) NOT NULL DEFAULT '';");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""Phone"" character varying(40) NOT NULL DEFAULT '';");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""ReferralCode"" character varying(24) NOT NULL DEFAULT '';");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Users"" ADD COLUMN IF NOT EXISTS ""ReferredByUserId"" uuid NULL;");

        // مهم: بعض العملاء ينشئون حساب برقم هاتف بدون إيميل، فالإيميل يبقى فارغاً.
        // الفهرس القديم الفريد على Email كان يمنع أكثر من حساب فارغ ويسبب خطأ 23505.
        await db.Database.ExecuteSqlRawAsync(@"DROP INDEX IF EXISTS ""IX_Users_Email"";");
        await db.Database.ExecuteSqlRawAsync(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"") WHERE ""Email"" IS NOT NULL AND btrim(""Email"") <> '';");

        await db.Database.ExecuteSqlRawAsync(@"UPDATE ""Users"" SET ""ReferralCode"" = 'DSB' || upper(substr(md5(""Id""::text), 1, 8)) WHERE ""ReferralCode"" IS NULL OR btrim(""ReferralCode"") = '';");
        await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Users_Phone"" ON ""Users"" (""Phone"");");
        await db.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_Users_ReferralCode"" ON ""Users"" (""ReferralCode"");");

        await db.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS ""PointsWallets"" (
            ""Id"" uuid PRIMARY KEY,
            ""UserId"" uuid NOT NULL,
            ""Balance"" integer NOT NULL DEFAULT 0,
            ""LifetimeEarned"" integer NOT NULL DEFAULT 0,
            ""LifetimeSpent"" integer NOT NULL DEFAULT 0,
            ""UpdatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now()
        );");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""PointsWallets"" ADD COLUMN IF NOT EXISTS ""LifetimeEarned"" integer NOT NULL DEFAULT 0;");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""PointsWallets"" ADD COLUMN IF NOT EXISTS ""LifetimeSpent"" integer NOT NULL DEFAULT 0;");
        await db.Database.ExecuteSqlRawAsync(@"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PointsWallets_UserId"" ON ""PointsWallets"" (""UserId"");");

        await db.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS ""Referrals"" (
            ""Id"" uuid PRIMARY KEY,
            ""ReferrerUserId"" uuid NOT NULL,
            ""ReferredUserId"" uuid NOT NULL,
            ""ReferralCode"" character varying(24) NOT NULL DEFAULT '',
            ""Status"" text NOT NULL DEFAULT 'Registered',
            ""Rewarded"" boolean NOT NULL DEFAULT FALSE,
            ""RewardType"" character varying(40) NULL,
            ""RewardPoints"" integer NOT NULL DEFAULT 0,
            ""RewardCouponCode"" character varying(80) NULL,
            ""CreatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now(),
            ""RewardedAtUtc"" timestamp with time zone NULL
        );");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Referrals"" ADD COLUMN IF NOT EXISTS ""RewardType"" character varying(40) NULL;");

        await db.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS ""UserGifts"" (
            ""Id"" uuid PRIMARY KEY,
            ""UserId"" uuid NOT NULL,
            ""ReferralId"" uuid NULL,
            ""GiftType"" character varying(40) NOT NULL DEFAULT 'Notice',
            ""Title"" character varying(160) NOT NULL DEFAULT '',
            ""Message"" text NOT NULL DEFAULT '',
            ""Points"" integer NOT NULL DEFAULT 0,
            ""CouponCode"" character varying(80) NULL,
            ""IsRead"" boolean NOT NULL DEFAULT FALSE,
            ""CreatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now()
        );");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""UserGifts"" ADD COLUMN IF NOT EXISTS ""ReferralId"" uuid NULL;");
    }

    private async Task EnsureWelcomeAdSchemaAsync()
    {
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Ads"" ADD COLUMN IF NOT EXISTS ""IsNewUserOnly"" boolean NOT NULL DEFAULT FALSE;");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Ads"" ADD COLUMN IF NOT EXISTS ""WelcomeCouponCode"" character varying(80) NULL;");
        await db.Database.ExecuteSqlRawAsync(@"ALTER TABLE IF EXISTS ""Ads"" ADD COLUMN IF NOT EXISTS ""WelcomePoints"" integer NOT NULL DEFAULT 0;");
    }

    private async Task<Ad?> GetActiveWelcomeOfferAsync()
    {
        var now = DateTimeOffset.UtcNow;
        // بعض النسخ القديمة خزنت إعلان الترحيب كـ Popup عادي بمكان ظهور "/"
        // لذلك لا نعتمد على Placement فقط؛ أي إعلان يحتوي IsNewUserOnly أو نقاط/كوبون ترحيبي يعتبر إعلان ترحيب.
        return await db.Ads.AsNoTracking()
            .Where(x => x.IsEnabled
                && x.Type == AdType.Popup
                && (x.IsNewUserOnly
                    || x.Placement == "welcome_new_user"
                    || x.WelcomePoints > 0
                    || (x.WelcomeCouponCode != null && x.WelcomeCouponCode.Trim() != ""))
                && (x.StartAt == null || x.StartAt <= now)
                && (x.EndAt == null || x.EndAt >= now))
            .OrderBy(x => x.IsNewUserOnly || x.Placement == "welcome_new_user" ? 0 : 1)
            .ThenBy(x => x.SortOrder)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    private static string BuildWelcomeMessage(Ad offer)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(offer.Subtitle)) parts.Add(offer.Subtitle.Trim());
        if (offer.WelcomePoints > 0) parts.Add($"حصلت على {offer.WelcomePoints} نقاط هدية.");
        if (!string.IsNullOrWhiteSpace(offer.WelcomeCouponCode)) parts.Add($"كود الخصم: {offer.WelcomeCouponCode}");
        if (parts.Count == 0) parts.Add("مبروك! حصلت على هدية ترحيبية، استمتع بالتسوق داخل التطبيق.");
        return string.Join(" ", parts);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var text = ex.ToString();
        return text.Contains("23505") || text.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
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
            new Claim(ClaimTypes.Name, !string.IsNullOrWhiteSpace(user.Email) ? user.Email : (!string.IsNullOrWhiteSpace(user.Phone) ? user.Phone : user.Id.ToString())),
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
