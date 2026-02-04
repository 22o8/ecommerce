// Ecommerce.Api/Infrastructure/Data/DbBootstrapper.cs
// Prevent common production 500s caused by missing columns/tables when migrations were not applied.
// We patch the minimal schema pieces required by: Products/AdminProducts + brand delete checks.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Api.Infrastructure.Data;

public static class DbBootstrapper
{
    public static async Task EnsureCoreSchemaAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // If database is unreachable, let it throw — better to fail loud.
        await db.Database.OpenConnectionAsync(ct);
        await db.Database.CloseConnectionAsync();

        // Products table: add missing columns used across the app.
        await TryExecAsync(db, logger, @"
ALTER TABLE IF EXISTS \"Products\" 
  ADD COLUMN IF NOT EXISTS \"Brand\" character varying(120) NOT NULL DEFAULT 'Unspecified';
", ct);

        await TryExecAsync(db, logger, @"
ALTER TABLE IF EXISTS \"Products\" 
  ADD COLUMN IF NOT EXISTS \"IsPublished\" boolean NOT NULL DEFAULT TRUE;
", ct);

        await TryExecAsync(db, logger, @"
ALTER TABLE IF EXISTS \"Products\" 
  ADD COLUMN IF NOT EXISTS \"RatingAvg\" numeric(3,2) NOT NULL DEFAULT 0;
", ct);

        await TryExecAsync(db, logger, @"
ALTER TABLE IF EXISTS \"Products\" 
  ADD COLUMN IF NOT EXISTS \"RatingCount\" integer NOT NULL DEFAULT 0;
", ct);

        // ProductImages table (admin/product details rely on it)
        await TryExecAsync(db, logger, @"
CREATE TABLE IF NOT EXISTS \"ProductImages\" (
  \"Id\" uuid NOT NULL,
  \"ProductId\" uuid NOT NULL,
  \"Url\" character varying(2000) NOT NULL,
  \"Alt\" character varying(400) NULL,
  \"SortOrder\" integer NOT NULL DEFAULT 0,
  CONSTRAINT \"PK_ProductImages\" PRIMARY KEY (\"Id\"),
  CONSTRAINT \"FK_ProductImages_Products_ProductId\" FOREIGN KEY (\"ProductId\") REFERENCES \"Products\"(\"Id\") ON DELETE CASCADE
);
", ct);

        await TryExecAsync(db, logger, @"
CREATE INDEX IF NOT EXISTS \"IX_ProductImages_ProductId\" ON \"ProductImages\" (\"ProductId\");
", ct);
    }

    private static async Task TryExecAsync(AppDbContext db, ILogger logger, string sql, CancellationToken ct)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DB bootstrap SQL failed. SQL: {Sql}", sql);
        }
    }
}
