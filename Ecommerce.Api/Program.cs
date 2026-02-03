using System.Text;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ============================
// Services
// ============================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // ✅ يمنع تعارض أسماء الـ Schemas
    c.CustomSchemaIds(t => t.FullName!.Replace("+", "."));

    // ✅ Swagger Authorize (JWT Bearer)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "اكتب بهذا الشكل: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================
// DbContext (Connection String Fix)
// ============================

static string NormalizeConn(string raw)
{
    raw = (raw ?? "").Trim();

    // إذا واحد مخزنها بالخطأ: psql 'postgresql://...'
    if (raw.StartsWith("psql", StringComparison.OrdinalIgnoreCase))
    {
        // remove leading "psql"
        raw = raw.Substring(4).Trim();
    }

    // remove wrapping quotes: '...' or "..."
    if ((raw.StartsWith("'") && raw.EndsWith("'")) || (raw.StartsWith("\"") && raw.EndsWith("\"")))
    {
        raw = raw.Substring(1, raw.Length - 2).Trim();
    }

    return raw;
}

var connRaw =
    builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration["ConnectionStrings__Default"]
    ?? builder.Configuration["ConnectionStrings:Default"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings:Default")
    ?? "";

var conn = NormalizeConn(connRaw);

if (string.IsNullOrWhiteSpace(conn))
    throw new Exception("Missing connection string: ConnectionStrings:Default");

// لو تريد تشوفها باللوغ (بدون كشفها كاملة) تقدر تطبع جزء:
Console.WriteLine($"DB Conn set (len={conn.Length})");

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql(conn);
});

// ============================
// CORS
// ============================

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("cors", p =>
    {
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
         .SetIsOriginAllowed(_ => true);
    });
});

// ============================
// JWT Auth
// ============================

var jwtKey =
    config["Jwt:Key"]
    ?? config["JWT_SECRET"]
    ?? config["JWT_KEY"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? Environment.GetEnvironmentVariable("JWT_KEY")
    ?? Environment.GetEnvironmentVariable("Jwt__Key")
    ?? "DEV_ONLY_CHANGE_ME";

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ============================
// App
// ============================

var app = builder.Build();

// ✅ Forwarded Headers (Fly/Proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // أحياناً لازم حتى يقبل الهيدرز من البروكسي
    KnownNetworks = { },
    KnownProxies = { }
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS Redirect (اختياري - Fly عادة يضبطه)
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("cors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================
// Apply Migrations Toggle
// ============================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // افتراضياً: نطبّق الـ migrations حتى لا يصير (relation does not exist) على Fly/Neon.
    // إذا تريد توقفها: خلّي RUN_MIGRATIONS=0 أو false.
    var runMigrationsRaw = (Environment.GetEnvironmentVariable("RUN_MIGRATIONS") ?? "true")
        .Trim()
        .ToLowerInvariant();

    var runMigrations = !(runMigrationsRaw is "0" or "false" or "no" or "n" or "off");

    if (runMigrations)
    {
        Console.WriteLine("Applying EF migrations (RUN_MIGRATIONS default=ON)...");
        db.Database.Migrate();
        Console.WriteLine("EF migrations applied.");
    }
    else
    {
        Console.WriteLine("RUN_MIGRATIONS=OFF -> skipping EF migrations.");
    }
}

app.Run();
