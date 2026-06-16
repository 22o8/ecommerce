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

            // Discounts (Fixes 500s when the latest migration wasn't applied)
            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""DiscountPercent"" integer NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""RatingAvg"" numeric(3,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""RatingCount"" integer NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""ProblemCategory"" character varying(80) NOT NULL DEFAULT "";",

            @"ALTER TABLE IF EXISTS ""Products""
              ADD COLUMN IF NOT EXISTS ""ProblemSubCategory"" character varying(120) NOT NULL DEFAULT "";",

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

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""SoldAt"" timestamp with time zone NULL;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""ProfitIqd"" numeric(18,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""ProfitUsd"" numeric(18,2) NOT NULL DEFAULT 0;",

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

            // ============================
            // Appearance (themes + effects + ads)
            // ============================
            @"CREATE TABLE IF NOT EXISTS ""AppearanceConfigs"" (
                ""Id"" uuid NOT NULL,
                ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                ""EnabledThemesJson"" jsonb NOT NULL DEFAULT '[]'::jsonb,
                ""EnabledEffectsJson"" jsonb NOT NULL DEFAULT '[]'::jsonb,
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_AppearanceConfigs"" PRIMARY KEY (""Id"")
              );",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""SiteLogoUrl"" character varying(2048) NULL;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroEnabled"" boolean NOT NULL DEFAULT FALSE;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroVideoUrl"" character varying(2048) NULL;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroTitle"" character varying(180) NULL;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroSubtitle"" character varying(400) NULL;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroButtonText"" character varying(80) NULL;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroButtonUrl"" character varying(2048) NULL;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroSecondaryButtonText"" character varying(100) NULL;",

            @"ALTER TABLE IF EXISTS ""AppearanceConfigs""
              ADD COLUMN IF NOT EXISTS ""IntroSecondaryButtonUrl"" character varying(2048) NULL;",

            @"CREATE TABLE IF NOT EXISTS ""AppearanceAds"" (
                ""Id"" uuid NOT NULL,
                ""AppearanceConfigId"" uuid NOT NULL,
                ""Title"" character varying(120) NULL,
                ""Subtitle"" character varying(200) NULL,
                ""ImageUrl"" character varying(2048) NOT NULL,
                ""LinkUrl"" character varying(2048) NULL,
                ""SortOrder"" integer NOT NULL DEFAULT 0,
                ""IsEnabled"" boolean NOT NULL DEFAULT TRUE,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_AppearanceAds"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_AppearanceAds_AppearanceConfigs_AppearanceConfigId"" FOREIGN KEY (""AppearanceConfigId"")
                    REFERENCES ""AppearanceConfigs""(""Id"") ON DELETE CASCADE
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_AppearanceAds_AppearanceConfigId"" ON ""AppearanceAds"" (""AppearanceConfigId"");",

            // Standalone Ads (Fixes 500s on /api/admin/ads when latest migration was not applied)
            @"CREATE TABLE IF NOT EXISTS ""Ads"" (
                ""Id"" uuid NOT NULL,
                ""Type"" integer NOT NULL DEFAULT 2,
                ""Placement"" character varying(120) NOT NULL DEFAULT 'home_top',
                ""Title"" text NOT NULL DEFAULT '',
                ""Subtitle"" text NULL,
                ""ImageUrl"" character varying(2000) NOT NULL DEFAULT '',
                ""LinkUrl"" character varying(1000) NULL,
                ""ProductId"" uuid NULL,
                ""SortOrder"" integer NOT NULL DEFAULT 0,
                ""IsEnabled"" boolean NOT NULL DEFAULT TRUE,
                ""StartAt"" timestamp with time zone NULL,
                ""EndAt"" timestamp with time zone NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_Ads"" PRIMARY KEY (""Id"")
              );",

            @"ALTER TABLE IF EXISTS ""Ads""
              ADD COLUMN IF NOT EXISTS ""ImageUrlsJson"" jsonb NULL;",

            @"CREATE INDEX IF NOT EXISTS ""IX_Ads_Type_Placement_SortOrder""
              ON ""Ads"" (""Type"", ""Placement"", ""SortOrder"");",

            @"CREATE TABLE IF NOT EXISTS ""ProductReviews"" (
                ""Id"" uuid NOT NULL,
                ""ProductId"" uuid NOT NULL,
                ""UserId"" uuid NOT NULL,
                ""Rating"" integer NOT NULL DEFAULT 5,
                ""Comment"" character varying(1500) NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_ProductReviews"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_ProductReviews_Products_ProductId"" FOREIGN KEY (""ProductId"") REFERENCES ""Products""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_ProductReviews_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"") ON DELETE CASCADE
              );",

            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ProductReviews_ProductId_UserId""
              ON ""ProductReviews"" (""ProductId"", ""UserId"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_ProductReviews_ProductId_CreatedAt""
              ON ""ProductReviews"" (""ProductId"", ""CreatedAt"");",

            @"ALTER TABLE IF EXISTS ""ProductReviews""
              ADD COLUMN IF NOT EXISTS ""ReviewerName"" character varying(160) NULL;",

            @"ALTER TABLE IF EXISTS ""ProductReviews""
              ADD COLUMN IF NOT EXISTS ""IsVerifiedPurchase"" boolean NOT NULL DEFAULT FALSE;",

            @"ALTER TABLE IF EXISTS ""ProductReviews""
              ADD COLUMN IF NOT EXISTS ""ImageUrlsJson"" jsonb NULL;",

            @"ALTER TABLE IF EXISTS ""ProductReviews""
              ADD COLUMN IF NOT EXISTS ""Status"" character varying(40) NOT NULL DEFAULT 'Approved';",

            @"CREATE INDEX IF NOT EXISTS ""IX_ProductReviews_ProductId_Status_CreatedAt""
              ON ""ProductReviews"" (""ProductId"", ""Status"", ""CreatedAt"");",



            // ============================
            // Loyalty / referral / points schema
            // Fixes production 500s when the new loyalty migration was not applied yet.
            // ============================
            @"ALTER TABLE IF EXISTS ""Users""
              ADD COLUMN IF NOT EXISTS ""ReferralCode"" character varying(24) NOT NULL DEFAULT '';",

            @"ALTER TABLE IF EXISTS ""Users""
              ADD COLUMN IF NOT EXISTS ""ReferredByUserId"" uuid NULL;",

            @"ALTER TABLE IF EXISTS ""Users""
              ADD COLUMN IF NOT EXISTS ""Phone"" character varying(40) NOT NULL DEFAULT '';",

            @"UPDATE ""Users""
              SET ""ReferralCode"" = 'DSB' || upper(substr(md5(""Id""::text), 1, 8))
              WHERE ""ReferralCode"" IS NULL OR btrim(""ReferralCode"") = '';",

            @"CREATE INDEX IF NOT EXISTS ""IX_Users_Phone"" ON ""Users"" (""Phone"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_Users_ReferralCode"" ON ""Users"" (""ReferralCode"");",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""SubtotalUsd"" numeric(18,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""SubtotalIqd"" numeric(18,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""DiscountAmountUsd"" numeric(18,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""DiscountAmountIqd"" numeric(18,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""DeliveryFeeIqd"" numeric(18,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""DeliveryFeeUsd"" numeric(18,2) NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""CustomerNote"" character varying(1000) NULL;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""AdminNote"" character varying(1000) NULL;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""PointsEarned"" integer NOT NULL DEFAULT 0;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""PointsAwarded"" boolean NOT NULL DEFAULT FALSE;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""PointsAwardedAtUtc"" timestamp with time zone NULL;",

            @"ALTER TABLE IF EXISTS ""Orders""
              ADD COLUMN IF NOT EXISTS ""CouponCode"" character varying(80) NULL;",

            @"CREATE TABLE IF NOT EXISTS ""PointsWallets"" (
                ""Id"" uuid NOT NULL,
                ""UserId"" uuid NOT NULL,
                ""Balance"" integer NOT NULL DEFAULT 0,
                ""LifetimeEarned"" integer NOT NULL DEFAULT 0,
                ""LifetimeSpent"" integer NOT NULL DEFAULT 0,
                ""UpdatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_PointsWallets"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_PointsWallets_Users_UserId"" FOREIGN KEY (""UserId"")
                    REFERENCES ""Users""(""Id"") ON DELETE CASCADE
              );",

            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PointsWallets_UserId""
              ON ""PointsWallets"" (""UserId"");",

            @"INSERT INTO ""PointsWallets"" (""Id"", ""UserId"", ""Balance"", ""LifetimeEarned"", ""LifetimeSpent"", ""UpdatedAtUtc"")
              SELECT ""Id"", ""Id"", 0, 0, 0, now()
              FROM ""Users""
              ON CONFLICT DO NOTHING;",

            @"CREATE TABLE IF NOT EXISTS ""Referrals"" (
                ""Id"" uuid NOT NULL,
                ""ReferrerUserId"" uuid NOT NULL,
                ""ReferredUserId"" uuid NOT NULL,
                ""ReferralCode"" character varying(24) NOT NULL DEFAULT '',
                ""Status"" text NOT NULL DEFAULT 'Registered',
                ""Rewarded"" boolean NOT NULL DEFAULT FALSE,
                ""RewardType"" text NULL,
                ""RewardPoints"" integer NOT NULL DEFAULT 0,
                ""RewardCouponCode"" text NULL,
                ""CreatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now(),
                ""RewardedAtUtc"" timestamp with time zone NULL,
                CONSTRAINT ""PK_Referrals"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_Referrals_Users_ReferrerUserId"" FOREIGN KEY (""ReferrerUserId"")
                    REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_Referrals_Users_ReferredUserId"" FOREIGN KEY (""ReferredUserId"")
                    REFERENCES ""Users""(""Id"") ON DELETE CASCADE
              );",

            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Referrals_ReferredUserId""
              ON ""Referrals"" (""ReferredUserId"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_Referrals_ReferrerUserId_CreatedAtUtc""
              ON ""Referrals"" (""ReferrerUserId"", ""CreatedAtUtc"");",

            @"CREATE TABLE IF NOT EXISTS ""PointsTransactions"" (
                ""Id"" uuid NOT NULL,
                ""WalletId"" uuid NOT NULL,
                ""UserId"" uuid NOT NULL,
                ""OrderId"" uuid NULL,
                ""Type"" character varying(40) NOT NULL DEFAULT 'Manual',
                ""Points"" integer NOT NULL DEFAULT 0,
                ""Note"" character varying(1000) NOT NULL DEFAULT '',
                ""CreatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_PointsTransactions"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_PointsTransactions_PointsWallets_WalletId"" FOREIGN KEY (""WalletId"")
                    REFERENCES ""PointsWallets""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_PointsTransactions_Users_UserId"" FOREIGN KEY (""UserId"")
                    REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_PointsTransactions_Orders_OrderId"" FOREIGN KEY (""OrderId"")
                    REFERENCES ""Orders""(""Id"") ON DELETE SET NULL
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_PointsTransactions_OrderId"" ON ""PointsTransactions"" (""OrderId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_PointsTransactions_WalletId"" ON ""PointsTransactions"" (""WalletId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_PointsTransactions_UserId_CreatedAtUtc""
              ON ""PointsTransactions"" (""UserId"", ""CreatedAtUtc"");",

            @"CREATE TABLE IF NOT EXISTS ""UserGifts"" (
                ""Id"" uuid NOT NULL,
                ""UserId"" uuid NOT NULL,
                ""ReferralId"" uuid NULL,
                ""GiftType"" text NOT NULL DEFAULT 'Points',
                ""Points"" integer NOT NULL DEFAULT 0,
                ""CouponCode"" text NULL,
                ""Title"" character varying(160) NOT NULL DEFAULT 'هدية جديدة',
                ""Message"" character varying(1000) NOT NULL DEFAULT '',
                ""IsRead"" boolean NOT NULL DEFAULT FALSE,
                ""CreatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_UserGifts"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_UserGifts_Users_UserId"" FOREIGN KEY (""UserId"")
                    REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_UserGifts_Referrals_ReferralId"" FOREIGN KEY (""ReferralId"")
                    REFERENCES ""Referrals""(""Id"") ON DELETE SET NULL
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_UserGifts_ReferralId"" ON ""UserGifts"" (""ReferralId"");",
            @"CREATE INDEX IF NOT EXISTS ""IX_UserGifts_UserId_CreatedAtUtc""
              ON ""UserGifts"" (""UserId"", ""CreatedAtUtc"");",
            @"CREATE TABLE IF NOT EXISTS ""Categories"" (
                ""Id"" uuid NOT NULL,
                ""Key"" character varying(80) NOT NULL,
                ""NameAr"" character varying(120) NOT NULL,
                ""NameEn"" character varying(120) NULL,
                ""DescriptionAr"" character varying(300) NULL,
                ""DescriptionEn"" character varying(300) NULL,
                ""ImageUrl"" character varying(2000) NULL,
                ""Section"" character varying(30) NOT NULL DEFAULT 'regular',
                ""ParentId"" uuid NULL,
                ""HasDetailSections"" boolean NOT NULL DEFAULT FALSE,
                ""SortOrder"" integer NOT NULL DEFAULT 0,
                ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_Categories"" PRIMARY KEY (""Id"")
              );",

            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Categories_Key""
              ON ""Categories"" (""Key"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_Categories_SortOrder""
              ON ""Categories"" (""SortOrder"");",

            @"ALTER TABLE IF EXISTS ""Categories""
              ADD COLUMN IF NOT EXISTS ""Section"" character varying(30) NOT NULL DEFAULT 'regular';",

            @"ALTER TABLE IF EXISTS ""Categories""
              ADD COLUMN IF NOT EXISTS ""ParentId"" uuid NULL;",

            @"ALTER TABLE IF EXISTS ""Categories""
              ADD COLUMN IF NOT EXISTS ""HasDetailSections"" boolean NOT NULL DEFAULT FALSE;",

            @"CREATE INDEX IF NOT EXISTS ""IX_Categories_Section_SortOrder""
              ON ""Categories"" (""Section"", ""SortOrder"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_Categories_Section_ParentId_SortOrder""
              ON ""Categories"" (""Section"", ""ParentId"", ""SortOrder"");",

            // ============================
            // Product Packages schema
            // ============================
            @"CREATE TABLE IF NOT EXISTS ""ProductPackages"" (
                ""Id"" uuid NOT NULL,
                ""NameAr"" character varying(180) NOT NULL DEFAULT '',
                ""NameEn"" character varying(180) NOT NULL DEFAULT '',
                ""Slug"" character varying(160) NOT NULL DEFAULT '',
                ""ShortDescription"" text NOT NULL DEFAULT '',
                ""SeoDescription"" text NOT NULL DEFAULT '',
                ""Note"" text NOT NULL DEFAULT '',
                ""CoverUrl"" character varying(2000) NOT NULL DEFAULT '',
                ""MediaType"" character varying(20) NOT NULL DEFAULT 'image',
                ""OriginalPriceIqd"" numeric(18,2) NOT NULL DEFAULT 0,
                ""FinalPriceIqd"" numeric(18,2) NOT NULL DEFAULT 0,
                ""Category"" text NOT NULL DEFAULT '',
                ""ProblemCategory"" text NOT NULL DEFAULT '',
                ""IsPublished"" boolean NOT NULL DEFAULT TRUE,
                ""IsFeatured"" boolean NOT NULL DEFAULT FALSE,
                ""ShowInSlider"" boolean NOT NULL DEFAULT FALSE,
                ""SliderPlacement"" character varying(40) NOT NULL DEFAULT 'packages',
                ""SortOrder"" integer NOT NULL DEFAULT 0,
                ""SoldCount"" integer NOT NULL DEFAULT 0,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_ProductPackages"" PRIMARY KEY (""Id"")
              );",

            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ProductPackages_Slug""
              ON ""ProductPackages"" (""Slug"");",

            @"CREATE TABLE IF NOT EXISTS ""ProductPackageItems"" (
                ""Id"" uuid NOT NULL,
                ""ProductPackageId"" uuid NOT NULL,
                ""ProductId"" uuid NOT NULL,
                ""Quantity"" integer NOT NULL DEFAULT 1,
                ""SortOrder"" integer NOT NULL DEFAULT 0,
                CONSTRAINT ""PK_ProductPackageItems"" PRIMARY KEY (""Id""),
                CONSTRAINT ""FK_ProductPackageItems_ProductPackages_ProductPackageId"" FOREIGN KEY (""ProductPackageId"")
                    REFERENCES ""ProductPackages""(""Id"") ON DELETE CASCADE,
                CONSTRAINT ""FK_ProductPackageItems_Products_ProductId"" FOREIGN KEY (""ProductId"")
                    REFERENCES ""Products""(""Id"") ON DELETE RESTRICT
              );",

            @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ProductPackageItems_ProductPackageId_ProductId""
              ON ""ProductPackageItems"" (""ProductPackageId"", ""ProductId"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_ProductPackageItems_ProductId""
              ON ""ProductPackageItems"" (""ProductId"");",

            // ============================
            // Admin Activities schema
            // ============================
            @"CREATE TABLE IF NOT EXISTS ""AdminActivities"" (
                ""Id"" uuid NOT NULL,
                ""AdminUserId"" uuid NULL,
                ""AdminEmail"" character varying(220) NOT NULL DEFAULT '',
                ""Action"" character varying(80) NOT NULL DEFAULT '',
                ""EntityType"" character varying(80) NOT NULL DEFAULT '',
                ""EntityId"" character varying(120) NULL,
                ""Title"" text NOT NULL DEFAULT '',
                ""Details"" text NOT NULL DEFAULT '',
                ""MetadataJson"" text NULL,
                ""CreatedAtUtc"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_AdminActivities"" PRIMARY KEY (""Id"")
              );",

            @"CREATE INDEX IF NOT EXISTS ""IX_AdminActivities_CreatedAtUtc""
              ON ""AdminActivities"" (""CreatedAtUtc"");",

            @"CREATE INDEX IF NOT EXISTS ""IX_AdminActivities_EntityType_Action_CreatedAtUtc""
              ON ""AdminActivities"" (""EntityType"", ""Action"", ""CreatedAtUtc"");",


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
