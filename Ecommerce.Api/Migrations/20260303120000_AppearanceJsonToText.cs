using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    /// <summary>
    /// Revert AppearanceConfigs JSON columns back to TEXT.
    /// We store JSON strings (arrays) as text to avoid jsonb/text casting issues on inserts.
    /// </summary>
    public partial class AppearanceJsonToText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // jsonb -> text
            migrationBuilder.Sql(@"
                ALTER TABLE \"AppearanceConfigs\"
                ALTER COLUMN \"EnabledThemesJson\" TYPE text USING \"EnabledThemesJson\"::text;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE \"AppearanceConfigs\"
                ALTER COLUMN \"EnabledEffectsJson\" TYPE text USING \"EnabledEffectsJson\"::text;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // text -> jsonb
            migrationBuilder.Sql(@"
                ALTER TABLE \"AppearanceConfigs\"
                ALTER COLUMN \"EnabledThemesJson\" TYPE jsonb USING \"EnabledThemesJson\"::jsonb;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE \"AppearanceConfigs\"
                ALTER COLUMN \"EnabledEffectsJson\" TYPE jsonb USING \"EnabledEffectsJson\"::jsonb;
            ");
        }
    }
}
