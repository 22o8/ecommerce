using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddProductBrand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ إضافة عمود Brand للمنتجات بشكل Idempotent
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'Products'
          AND column_name  = 'Brand'
    ) THEN
        ALTER TABLE ""Products"" ADD ""Brand"" text NOT NULL DEFAULT 'Unspecified';
    END IF;
END $$;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'Products'
          AND column_name  = 'Brand'
    ) THEN
        ALTER TABLE ""Products"" DROP COLUMN ""Brand"";
    END IF;
END $$;
");
        }
    }
}
