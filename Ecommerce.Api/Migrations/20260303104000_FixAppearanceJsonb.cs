using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class FixAppearanceJsonb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // If the columns were created as text in older migrations, convert them to jsonb.
            migrationBuilder.AlterColumn<string>(
                name: "EnabledEffectsJson",
                table: "AppearanceConfigs",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
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
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "EnabledThemesJson",
                table: "AppearanceConfigs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }
    }
}
