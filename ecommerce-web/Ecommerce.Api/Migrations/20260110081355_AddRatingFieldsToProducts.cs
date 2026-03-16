using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingFieldsToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "RatingAvg",
                table: "Products",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ProductImages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingAvg",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ProductImages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }
    }
}
