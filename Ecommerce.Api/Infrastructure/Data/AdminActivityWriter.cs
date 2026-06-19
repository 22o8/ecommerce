using System.Security.Claims;
using System.Text.Json;
using Ecommerce.Api.Domain.Entities;

namespace Ecommerce.Api.Infrastructure.Data;

public static class AdminActivityWriter
{
    public static async Task LogAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        string action,
        string entityType,
        string? entityId,
        string title,
        string details = "",
        object? metadata = null,
        CancellationToken ct = default)
    {
        try
        {
            var email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? string.Empty;
            Guid? userId = null;
            var idRaw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("id");
            if (Guid.TryParse(idRaw, out var parsed)) userId = parsed;

            db.AdminActivities.Add(new AdminActivity
            {
                Id = Guid.NewGuid(),
                AdminUserId = userId,
                AdminEmail = email,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Title = title,
                Details = details,
                MetadataJson = metadata == null ? null : JsonSerializer.Serialize(metadata),
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // نشاط الإدارة لا يجب أن يكسر العملية الأساسية.
        }
    }
}
