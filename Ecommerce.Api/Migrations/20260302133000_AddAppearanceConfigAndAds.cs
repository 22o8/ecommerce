using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddAppearanceConfigAndAds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppearanceConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnabledThemesJson = table.Column<string>(type: "text", nullable: false),
                    EnabledEffectsJson = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppearanceConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppearanceAds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppearanceConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Subtitle = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LinkUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppearanceAds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppearanceAds_AppearanceConfigs_AppearanceConfigId",
                        column: x => x.AppearanceConfigId,
                        principalTable: "AppearanceConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppearanceAds_AppearanceConfigId_SortOrder",
                table: "AppearanceAds",
                columns: new[] { "AppearanceConfigId", "SortOrder" });

            // seed one config row to make GET simple
            migrationBuilder.InsertData(
                table: "AppearanceConfigs",
                columns: new[] { "Id", "EnabledThemesJson", "EnabledEffectsJson", "IsActive", "UpdatedAt" },
                values: new object[] { Guid.Parse("11111111-1111-1111-1111-111111111111"), "[]", "[]", true, DateTimeOffset.UtcNow });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppearanceAds");

            migrationBuilder.DropTable(
                name: "AppearanceConfigs");
        }
    }
}
