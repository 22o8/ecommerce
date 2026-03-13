using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddProductCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "serum");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "Products",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Category", table: "Products");
            migrationBuilder.DropColumn(name: "SubCategory", table: "Products");
        }
    }
}
