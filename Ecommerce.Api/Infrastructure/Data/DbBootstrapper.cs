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

            // ============================
            // Checkout / Orders schema
            // (Fixes 500s on /api/Checkout/cart when migrations weren't applied)
            // ============================

            // Orders
            @"CREATE TABLE IF NOT EXISTS ""Orders"" (
                ""Id"" uuid NOT NULL,
                ""UserId"" uuid NOT NULL,
                ""CustomerEmail"" character varying(320) NOT NULL DEFAULT '',
                ""TotalUsd"" numeric(18,2) NOT NULL DEFAULT 0,
                ""TotalIqd"" numeric(18,2) NOT NULL DEFAULT 0,
                ""Currency"" character varying(3) NOT NULL DEFAULT 'IQD',
                ""Status"" character varying(50) NOT NULL DEFAULT 'Pending',
                ""Notes"" text NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_Orders"" PRIMARY KEY (""Id"")
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_Orders_UserId"" ON ""Orders"" (""UserId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_Orders_CreatedAt"" ON ""Orders"" (""CreatedAt"");",

            // OrderItems
            @"CREATE TABLE IF NOT EXISTS ""OrderItems"" (
                ""Id"" uuid NOT NULL,
                ""OrderId"" uuid NOT NULL,
                ""ProductId"" uuid NOT NULL,
                ""Quantity"" integer NOT NULL DEFAULT 1,
                ""UnitPriceUsd"" numeric(18,2) NOT NULL DEFAULT 0,
                CONSTRAINT ""PK_OrderItems"" PRIMARY KEY (""Id""),
		                CONSTRAINT ""FK_OrderItems_Orders_OrderId"" FOREIGN KEY (""OrderId"")
                    REFERENCES ""Orders""(""Id"") ON DELETE CASCADE,
		                CONSTRAINT ""FK_OrderItems_Products_ProductId"" FOREIGN KEY (""ProductId"")
                    REFERENCES ""Products""(""Id"") ON DELETE RESTRICT
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_OrderItems_OrderId"" ON ""OrderItems"" (""OrderId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_OrderItems_ProductId"" ON ""OrderItems"" (""ProductId"");",

            // Payments
            @"CREATE TABLE IF NOT EXISTS ""Payments"" (
                ""Id"" uuid NOT NULL,
                ""OrderId"" uuid NOT NULL,
                ""AmountUsd"" numeric(18,2) NOT NULL DEFAULT 0,
                ""AmountIqd"" numeric(18,2) NOT NULL DEFAULT 0,
                ""Method"" character varying(40) NOT NULL DEFAULT 'Cash',
                ""Status"" character varying(40) NOT NULL DEFAULT 'Pending',
                ""ProviderRef"" character varying(200) NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_Payments"" PRIMARY KEY (""Id""),
		                CONSTRAINT ""FK_Payments_Orders_OrderId"" FOREIGN KEY (""OrderId"")
                    REFERENCES ""Orders""(""Id"") ON DELETE CASCADE
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_Payments_OrderId"" ON ""Payments"" (""OrderId"");",
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
