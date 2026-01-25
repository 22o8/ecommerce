using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.Api.Migrations
{
    [Migration("20260125000000_AddUserPhone")]
    public partial class AddUserPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            // Idempotent: لا يطيّح الديبلوي إذا العمود موجود مسبقاً
            // نخليه VARCHAR(30) حتى يطابق أغلب إعدادات الـ Entity (MaxLength 30)
            migrationBuilder.Sql(@"
=======
        // في بعض بيئات النشر (مثل Render) ممكن يكون العمود موجود أصلًا
        // بسبب تشغيل Migration سابق أو تعديل يدوي على قاعدة البيانات.
        // نخليها Idempotent حتى ما يطيح التطبيق بخطأ: column already exists
        migrationBuilder.Sql(@"
>>>>>>> bdd2dec (fix: resolve conflicts + ui cleanup + admin layout + cart)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'Users'
          AND column_name  = 'Phone'
    ) THEN
<<<<<<< HEAD
        ALTER TABLE ""Users"" ADD ""Phone"" character varying(30) NOT NULL DEFAULT '';
=======
        ALTER TABLE \"Users\" ADD \"Phone\" text NOT NULL DEFAULT '';
>>>>>>> bdd2dec (fix: resolve conflicts + ui cleanup + admin layout + cart)
    END IF;
END $$;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            migrationBuilder.Sql(@"
=======
        migrationBuilder.Sql(@"
>>>>>>> bdd2dec (fix: resolve conflicts + ui cleanup + admin layout + cart)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'Users'
          AND column_name  = 'Phone'
    ) THEN
<<<<<<< HEAD
        ALTER TABLE ""Users"" DROP COLUMN ""Phone"";
=======
        ALTER TABLE \"Users\" DROP COLUMN \"Phone\";
>>>>>>> bdd2dec (fix: resolve conflicts + ui cleanup + admin layout + cart)
    END IF;
END $$;
");
        }
    }
}
