using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    [Migration("20260125000000_AddUserPhone")]
    public partial class AddUserPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: لا يطيّح الديبلوي إذا العمود موجود مسبقاً
            // نخليه VARCHAR(30) حتى يطابق أغلب إعدادات الـ Entity (MaxLength 30)
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
        ALTER TABLE ""Users"" ADD ""Phone"" character varying(30) NOT NULL DEFAULT '';
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
