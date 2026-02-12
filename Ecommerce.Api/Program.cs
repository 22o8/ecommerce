using System.Text;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
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

// IMPORTANT: When running behind a reverse proxy (Fly/Cloudflare/Vercel/etc.) the app
// receives X-Forwarded-* headers. Without this middleware, Request.Scheme might be "http"
// which causes generated asset URLs to be invalid on an https site.
var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
// Accept forwarded headers from any proxy in this environment.
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

// ============================
// Global exception -> JSON (حتى ما يصير body فارغ مع 500)
// ============================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json; charset=utf-8";

        // trace id يفيدك تربط الخطأ باللوغ على Fly
        var traceId = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Internal Server Error",
            traceId
        });
    });
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS Redirect (اختياري - Fly عادة يضبطه)
app.UseHttpsRedirection();

// Serve /wwwroot if present
app.UseStaticFiles();

// Serve uploaded files from ContentRootPath/uploads at /uploads
// (e.g. /uploads/brands/{brandId}/logo.png)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

// ✅ Return a placeholder image instead of 404 for missing uploads
// This prevents broken thumbnails on the UI when the physical file is missing
// (e.g. Fly volume reset, seed data without files, etc.).
// We still keep the real file serving via StaticFiles below.
var placeholderPng = Convert.FromBase64String(
    // 1x1 transparent PNG
    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMB/6X2qzUAAAAASUVORK5CYII="
);

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/uploads", out var remaining))
    {
        var rel = (remaining.Value ?? string.Empty).TrimStart('/');
        if (!string.IsNullOrWhiteSpace(rel))
        {
            // Normalize path separators to avoid path traversal
            rel = rel.Replace("..", string.Empty)
                     .Replace('\\', '/')
                     .TrimStart('/');

            var fullPath = Path.Combine(uploadsPath, rel.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "image/png";
                await context.Response.Body.WriteAsync(placeholderPng);
                return;
            }
        }
    }

    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseCors("cors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================
// ✅ Apply EF Migrations (Auto)
// ============================
// المشكلة اللي عندك (500 على /api/admin/products و DELETE brand) غالباً بسبب إن
// قاعدة البيانات على Fly ما مطبّقة آخر Migrations (مثلاً عمود Product.Brand).
// عشان ما تعتمد على إعدادات يدوية، نخلي التطبيق يطبّق المايغريشن تلقائياً
// إلا إذا حبيت توقفه بإضافة: SKIP_MIGRATIONS=true

try
{
    var skipRaw = (Environment.GetEnvironmentVariable("SKIP_MIGRATIONS") ?? "")
        .Trim()
        .ToLowerInvariant();

    var skipMigrations = skipRaw is "1" or "true" or "yes" or "y" or "on";

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!skipMigrations)
    {
        Console.WriteLine("Applying EF migrations (auto)...");
        db.Database.Migrate();
        Console.WriteLine("EF migrations applied.");
    }
    else
    {
        Console.WriteLine("SKIP_MIGRATIONS enabled -> skipping EF migrations.");
    }

    // ✅ حتى لو ماكو Migrations داخل المشروع/أو ما تنطبق، نصلّح أقل سكيمة مطلوبة
    // هذا يحل غالباً 500 بـ AdminProducts + حذف البراند.
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbBootstrapper");
    DbBootstrapper.EnsureCoreSchemaAsync(db, logger).GetAwaiter().GetResult();
}
catch (Exception ex)
{
    // لا نسقط التطبيق إذا فشل المايغريشن، حتى تقدر تشوف اللوج وتصلح DB لاحقاً.
    Console.WriteLine("❌ Failed to apply EF migrations/bootstrap: " + ex);
}

app.Run();
