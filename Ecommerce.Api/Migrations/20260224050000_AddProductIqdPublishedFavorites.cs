using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    /// <summary>
    /// Adds missing Product columns (PriceIqd, IsPublished) and creates Favorites table.
    /// Written with PostgreSQL guards to be safe on existing DBs.
    /// </summary>
    public partial class AddProductIqdPublishedFavorites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Products: add missing columns (safe if they already exist)
            migrationBuilder.Sql(@"
                -- PriceIqd
                ALTER TABLE \"Products\"
                ADD COLUMN IF NOT EXISTS \"PriceIqd\" numeric NOT NULL DEFAULT 0;

                -- If the column existed but was nullable, normalize nulls then enforce NOT NULL.
                UPDATE \"Products\" SET \"PriceIqd\" = 0 WHERE \"PriceIqd\" IS NULL;
                ALTER TABLE \"Products\" ALTER COLUMN \"PriceIqd\" SET NOT NULL;

                -- IsPublished
                ALTER TABLE \"Products\"
                ADD COLUMN IF NOT EXISTS \"IsPublished\" boolean NOT NULL DEFAULT TRUE;

                UPDATE \"Products\" SET \"IsPublished\" = TRUE WHERE \"IsPublished\" IS NULL;
                ALTER TABLE \"Products\" ALTER COLUMN \"IsPublished\" SET NOT NULL;
            ");

            // Favorites table (matches Domain.Entities.Favorite)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS \"Favorites\" (
                    \"Id\" uuid NOT NULL,
                    \"UserId\" uuid NOT NULL,
                    \"ProductId\" uuid NOT NULL,
                    \"CreatedAt\" timestamptz NOT NULL DEFAULT now(),
                    CONSTRAINT \"PK_Favorites\" PRIMARY KEY (\"Id\")
                );

                -- prevent duplicates per user/product
                CREATE UNIQUE INDEX IF NOT EXISTS \"UX_Favorites_User_Product\" ON \"Favorites\" (\"UserId\", \"ProductId\");
                CREATE INDEX IF NOT EXISTS \"IX_Favorites_ProductId\" ON \"Favorites\" (\"ProductId\");

                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Favorites_Users_UserId') THEN
                        ALTER TABLE \"Favorites\" ADD CONSTRAINT \"FK_Favorites_Users_UserId\" FOREIGN KEY (\"UserId\") REFERENCES \"Users\"(\"Id\") ON DELETE CASCADE;
                    END IF;

                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Favorites_Products_ProductId') THEN
                        ALTER TABLE \"Favorites\" ADD CONSTRAINT \"FK_Favorites_Products_ProductId\" FOREIGN KEY (\"ProductId\") REFERENCES \"Products\"(\"Id\") ON DELETE CASCADE;
                    END IF;
                END$$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS \"Favorites\";");

            migrationBuilder.Sql(@"
                ALTER TABLE \"Products\" DROP COLUMN IF EXISTS \"PriceIqd\";
                ALTER TABLE \"Products\" DROP COLUMN IF EXISTS \"IsPublished\";
            ");
        }
    }
}
