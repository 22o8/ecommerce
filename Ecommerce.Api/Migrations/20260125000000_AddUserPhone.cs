using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.Api.Migrations
{
    [Migration("20260125000000_AddUserPhone")]
    public partial class AddUserPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        // في بعض بيئات النشر (مثل Render) ممكن يكون العمود موجود أصلًا
        // بسبب تشغيل Migration سابق أو تعديل يدوي على قاعدة البيانات.
        // نخليها Idempotent حتى ما يطيح التطبيق بخطأ: column already exists
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
        ALTER TABLE \"Users\" ADD \"Phone\" text NOT NULL DEFAULT '';
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
        ALTER TABLE \"Users\" DROP COLUMN \"Phone\";
    END IF;
END $$;
");
        }
    }
}
