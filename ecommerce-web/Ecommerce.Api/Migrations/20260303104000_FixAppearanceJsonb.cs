using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class FixAppearanceJsonb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IMPORTANT:
            // These columns are stored as jsonb in Postgres and are mapped as JsonDocument in the entity.
            // If a migration uses string as CLR type, EF will send text parameters and Postgres will throw:
            // "column ... is of type jsonb but expression is of type text".
            migrationBuilder.AlterColumn<JsonDocument>(
                name: "EnabledEffectsJson",
                table: "AppearanceConfigs",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "EnabledThemesJson",
                table: "AppearanceConfigs",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EnabledEffectsJson",
                table: "AppearanceConfigs",
                type: "text",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "EnabledThemesJson",
                table: "AppearanceConfigs",
                type: "text",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb");
        }
    }
}
