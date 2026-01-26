using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddUserPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: لا يفشل إذا العمود موجود مسبقاً
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'Users'
          AND column_name  = 'Phone'
    ) THEN
        ALTER TABLE ""Users"" ADD ""Phone"" text NOT NULL DEFAULT '';
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
          AND table_name   = 'Users'
          AND column_name  = 'Phone'
    ) THEN
        ALTER TABLE ""Users"" DROP COLUMN ""Phone"";
    END IF;
END $$;
");
        }
    }
}
