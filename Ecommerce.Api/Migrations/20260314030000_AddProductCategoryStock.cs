using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddProductCategoryStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "text",
                maxLength: 80,
                nullable: false,
                defaultValue: "general");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "Products",
                type: "text",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.Sql("UPDATE \"Products\" SET \"Category\" = COALESCE(NULLIF(\"Category\", ''), 'general')");
            migrationBuilder.Sql("UPDATE \"Products\" SET \"StockQuantity\" = CASE WHEN \"StockQuantity\" < 0 THEN 0 ELSE \"StockQuantity\" END");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category_SubCategory_IsPublished",
                table: "Products",
                columns: new[] { "Category", "SubCategory", "IsPublished" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Category_SubCategory_IsPublished",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "Products");
        }
    }
}
