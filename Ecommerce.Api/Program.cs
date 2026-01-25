using System.Text;
using Ecommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// اختصار للكونفيغ حتى ما يصير خطأ (config غير معرّف / مكرر)
var config = builder.Configuration;

// ============================
// Services
// ============================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // ✅ يمنع تعارض أسماء الـ Schemas لما يكون عندك DTOs/Requests بنفس الاسم
    // في أكثر من مكان. هذا كان يسبب 500 في /swagger/v1/swagger.json على Render.
    c.CustomSchemaIds(t => t.FullName);
});

// DbContext
var conn = config.GetConnectionString("Default")
          ?? config["ConnectionStrings__Default"];

if (string.IsNullOrWhiteSpace(conn))
    throw new Exception("Missing connection string: ConnectionStrings:Default");

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql(conn);
});

// CORS (عدّل origins حسب موقعك)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("cors", p =>
    {
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
         // اذا تبي تربط دومين الفرونت المحدد بدل AllowAnyOrigin:
         .SetIsOriginAllowed(_ => true);
    });
});

// JWT Auth (إذا عندك Secret بالكونفيغ)
var jwtKey = config["Jwt:Key"] ?? config["JWT_KEY"];

if (!string.IsNullOrWhiteSpace(jwtKey))
{
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
}
else
{
    // اذا أنت أصلاً مستخدم JWT بطريقة ثانية/جاهزة.. اترك هذا حسب مشروعك
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

// ============================
// App
// ============================

var app = builder.Build();

// تطبيق المايغريشن تلقائياً (مهم للديبلوي على Render)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Migrations");

        logger.LogError(ex, "Database migration failed");
        // لا نكسر التطبيق؛ راح تبين المشكلة في اللوغ
    }
}

// Render / Reverse Proxy Support (مهم)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS (إذا أنت مفعّل https بالبيئة)
app.UseHttpsRedirection();

// Static Files (ضروري حتى /uploads يفتح)
app.UseStaticFiles();

app.UseRouting();

// CORS
app.UseCors("cors");

// Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
