using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddDiscountsAndStandaloneAds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Ads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Placement = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Subtitle = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LinkUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ads", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ads_Type_Placement_SortOrder",
                table: "Ads",
                columns: new[] { "Type", "Placement", "SortOrder" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ads");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Products");
        }
    }
}
