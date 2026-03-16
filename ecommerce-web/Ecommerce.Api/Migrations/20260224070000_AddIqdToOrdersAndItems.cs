using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddIqdToOrdersAndItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalIqd",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceIqd",
                table: "OrderItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotalIqd",
                table: "OrderItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalIqd",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UnitPriceIqd",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LineTotalIqd",
                table: "OrderItems");
        }
    }
}
