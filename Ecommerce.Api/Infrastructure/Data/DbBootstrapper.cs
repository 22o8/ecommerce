// Ecommerce.Api/Infrastructure/Data/DbBootstrapper.cs
// Prevent common production 500s caused by missing columns/tables when migrations were not applied.
// This is a lightweight "bootstrap" (idempotent) to keep the app running even if DB migrations were skipped.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Api.Infrastructure.Data;

public static class DbBootstrapper
{
    public static async Task EnsureCoreSchemaAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        // If database is unreachable, let it throw — better to fail loud early.
        await db.Database.OpenConnectionAsync(ct);
        await db.Database.CloseConnectionAsync();

        var statements = new[]
        {
            // Products table: columns used by the app
            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""Brand"" character varying(120) NOT NULL DEFAULT 'Unspecified';",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""IsPublished"" boolean NOT NULL DEFAULT TRUE;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""IsFeatured"" boolean NOT NULL DEFAULT FALSE;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""RatingAvg"" numeric(3,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""RatingCount"" integer NOT NULL DEFAULT 0;",

            // Prices (IQD is primary)
            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""PriceIqd"" numeric NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""PriceUsd"" numeric NOT NULL DEFAULT 0;",

            // Optional cached counters (not required for analytics queries but useful in UI)
            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""ViewsCount"" integer NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""FavoritesCount"" integer NOT NULL DEFAULT 0;",

            // Favorites table (server-side wishlist)
            @"CREATE TABLE IF NOT EXISTS ""Favorites"" (
                ""Id"" uuid NOT NULL,
                ""UserId"" uuid NOT NULL,
                ""ProductId"" uuid NOT NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL,
                CONSTRAINT ""PK_Favorites"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_Favorites_Users_UserId"" FOREIGN KEY (""UserId"")
                    REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_Favorites_Products_ProductId"" FOREIGN KEY (""ProductId"")
                    REFERENCES ""Products""(""Id"") ON DELETE CASCADE
              );",

            @"CREATE UNIQUE INDEX IF NOT EXISTS ""UX_Favorites_User_Product""
              ON ""Favorites"" (""UserId"", ""ProductId"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_Favorites_ProductId""
              ON ""Favorites"" (""ProductId"");",

            // Product views table (track every modal open)
            @"CREATE TABLE IF NOT EXISTS ""ProductViews"" (
                ""Id"" uuid NOT NULL,
                ""ProductId"" uuid NOT NULL,
                ""UserId"" uuid NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL,
                CONSTRAINT ""PK_ProductViews"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_ProductViews_Products_ProductId"" FOREIGN KEY (""ProductId"")
                    REFERENCES ""Products""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_ProductViews_Users_UserId"" FOREIGN KEY (""UserId"")
                    REFERENCES ""Users""(""Id"") ON DELETE SET NULL
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_ProductViews_ProductId""
              ON ""ProductViews"" (""ProductId"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_ProductViews_CreatedAt""
              ON ""ProductViews"" (""CreatedAt"");",

            // ProductImages table (admin/product details rely on it)
            @"CREATE TABLE IF NOT EXISTS ""ProductImages"" (
                ""Id"" uuid NOT NULL,
                ""ProductId"" uuid NOT NULL,
                ""Url"" character varying(2000) NOT NULL,
                ""Alt"" character varying(400) NULL,
                ""SortOrder"" integer NOT NULL DEFAULT 0,
                CONSTRAINT ""PK_ProductImages"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_ProductImages_Products_ProductId"" FOREIGN KEY (""ProductId"")
                    REFERENCES ""Products""(""Id"") ON DELETE CASCADE
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_ProductImages_ProductId""
              ON ""ProductImages"" (""ProductId"");",
        };

        foreach (var sql in statements)
        {
            await TryExecAsync(db, logger, sql, ct);
        }
    }

    private static async Task TryExecAsync(AppDbContext db, ILogger logger, string sql, CancellationToken ct)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }
        catch (Exception ex)
        {
            // Don't crash the whole app just because a bootstrap statement failed.
            // Log for troubleshooting.
            logger.LogError(ex, "DB bootstrap SQL failed. SQL: {Sql}", sql);
        }
    }
}
