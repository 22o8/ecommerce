using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddCouponEligibilityAndUsages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCouponAllowed",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "CouponUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceKeyHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponUsages", x => x.Id);
                    table.ForeignKey(name: "FK_CouponUsages_Coupons_CouponId", column: x => x.CouponId, principalTable: "Coupons", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_CouponUsages_Orders_OrderId", column: x => x.OrderId, principalTable: "Orders", principalColumn: "Id");
                    table.ForeignKey(name: "FK_CouponUsages_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId_DeviceKeyHash",
                table: "CouponUsages",
                columns: new[] { "CouponId", "DeviceKeyHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId_UserId",
                table: "CouponUsages",
                columns: new[] { "CouponId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_OrderId",
                table: "CouponUsages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CouponUsages");
            migrationBuilder.DropColumn(name: "IsCouponAllowed", table: "Products");
        }
    }
}
