using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations;

public partial class ProductReviewsProAndAnalytics : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ReviewerName",
            table: "ProductReviews",
            type: "character varying(160)",
            maxLength: 160,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsVerifiedPurchase",
            table: "ProductReviews",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "ImageUrlsJson",
            table: "ProductReviews",
            type: "jsonb",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "ProductReviews",
            type: "character varying(40)",
            maxLength: 40,
            nullable: false,
            defaultValue: "Approved");

        migrationBuilder.CreateIndex(
            name: "IX_ProductReviews_ProductId_Status_CreatedAt",
            table: "ProductReviews",
            columns: new[] { "ProductId", "Status", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ProductReviews_ProductId_Status_CreatedAt",
            table: "ProductReviews");

        migrationBuilder.DropColumn(name: "ReviewerName", table: "ProductReviews");
        migrationBuilder.DropColumn(name: "IsVerifiedPurchase", table: "ProductReviews");
        migrationBuilder.DropColumn(name: "ImageUrlsJson", table: "ProductReviews");
        migrationBuilder.DropColumn(name: "Status", table: "ProductReviews");
    }
}
