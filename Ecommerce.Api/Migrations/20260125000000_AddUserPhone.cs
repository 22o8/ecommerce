using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddUserPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: لا يفشل إذا العمود موجود مسبقاً
<<<<<<< HEAD
            migrationBuilder.Sql("""
DO $$
=======
            migrationBuilder.Sql(@"DO $$
>>>>>>> 2f20a05 (fix: admin middleware SSR safe + translations + composables)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'Users'
          AND column_name  = 'Phone'
    ) THEN
<<<<<<< HEAD
        ALTER TABLE "Users" ADD "Phone" text NOT NULL DEFAULT '';
    END IF;
END $$;
""");
=======
        ALTER TABLE ""Users"" ADD ""Phone"" text NOT NULL DEFAULT '';
    END IF;
END $$;");
>>>>>>> 2f20a05 (fix: admin middleware SSR safe + translations + composables)
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
<<<<<<< HEAD
            migrationBuilder.Sql("""
DO $$
=======
            migrationBuilder.Sql(@"DO $$
>>>>>>> 2f20a05 (fix: admin middleware SSR safe + translations + composables)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name   = 'Users'
          AND column_name  = 'Phone'
    ) THEN
<<<<<<< HEAD
        ALTER TABLE "Users" DROP COLUMN "Phone";
    END IF;
END $$;
""");
=======
        ALTER TABLE ""Users"" DROP COLUMN ""Phone"";
    END IF;
END $$;");
>>>>>>> 2f20a05 (fix: admin middleware SSR safe + translations + composables)
        }
    }
}
