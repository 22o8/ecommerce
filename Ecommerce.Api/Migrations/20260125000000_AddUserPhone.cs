using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.Api.Migrations
{
    public partial class AddUserPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make migration idempotent for existing databases.
            // Render DB already has the column but migrations history may not.
            migrationBuilder.Sql("ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"Phone\" text NOT NULL DEFAULT '';" );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"Phone\";");
        }
    }
}
