using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    public partial class AddLoyaltyReferralAndOrderNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "ReferralCode", table: "Users", type: "character varying(24)", maxLength: 24, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<Guid>(name: "ReferredByUserId", table: "Users", type: "uuid", nullable: true);
            migrationBuilder.Sql("UPDATE \"Users\" SET \"ReferralCode\" = 'DSB' || upper(substr(md5(\"Id\"::text), 1, 8)) WHERE \"ReferralCode\" IS NULL OR \"ReferralCode\" = '';");
            migrationBuilder.AddColumn<decimal>(name: "DeliveryFeeIqd", table: "Orders", type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "DeliveryFeeUsd", table: "Orders", type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<string>(name: "CustomerNote", table: "Orders", type: "character varying(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<string>(name: "AdminNote", table: "Orders", type: "character varying(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<int>(name: "PointsEarned", table: "Orders", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<bool>(name: "PointsAwarded", table: "Orders", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<DateTime>(name: "PointsAwardedAtUtc", table: "Orders", type: "timestamp with time zone", nullable: true);

            migrationBuilder.CreateTable(name: "PointsWallets", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Balance = table.Column<int>(type: "integer", nullable: false),
                LifetimeEarned = table.Column<int>(type: "integer", nullable: false),
                LifetimeSpent = table.Column<int>(type: "integer", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PointsWallets", x => x.Id);
                table.ForeignKey("FK_PointsWallets_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "Referrals", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ReferrerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                ReferredUserId = table.Column<Guid>(type: "uuid", nullable: false),
                ReferralCode = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                Rewarded = table.Column<bool>(type: "boolean", nullable: false),
                RewardType = table.Column<string>(type: "text", nullable: true),
                RewardPoints = table.Column<int>(type: "integer", nullable: false),
                RewardCouponCode = table.Column<string>(type: "text", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                RewardedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Referrals", x => x.Id);
                table.ForeignKey("FK_Referrals_Users_ReferredUserId", x => x.ReferredUserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Referrals_Users_ReferrerUserId", x => x.ReferrerUserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "PointsTransactions", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                Points = table.Column<int>(type: "integer", nullable: false),
                Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PointsTransactions", x => x.Id);
                table.ForeignKey("FK_PointsTransactions_Orders_OrderId", x => x.OrderId, "Orders", "Id");
                table.ForeignKey("FK_PointsTransactions_PointsWallets_WalletId", x => x.WalletId, "PointsWallets", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_PointsTransactions_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "UserGifts", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ReferralId = table.Column<Guid>(type: "uuid", nullable: true),
                GiftType = table.Column<string>(type: "text", nullable: false),
                Points = table.Column<int>(type: "integer", nullable: false),
                CouponCode = table.Column<string>(type: "text", nullable: true),
                Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                IsRead = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_UserGifts", x => x.Id);
                table.ForeignKey("FK_UserGifts_Referrals_ReferralId", x => x.ReferralId, "Referrals", "Id");
                table.ForeignKey("FK_UserGifts_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_Users_Phone", "Users", "Phone");
            migrationBuilder.CreateIndex("IX_Users_ReferralCode", "Users", "ReferralCode");
            migrationBuilder.CreateIndex("IX_PointsWallets_UserId", "PointsWallets", "UserId", unique: true);
            migrationBuilder.Sql("INSERT INTO \"PointsWallets\" (\"Id\", \"UserId\", \"Balance\", \"LifetimeEarned\", \"LifetimeSpent\", \"UpdatedAtUtc\") SELECT \"Id\", \"Id\", 0, 0, 0, now() FROM \"Users\" ON CONFLICT DO NOTHING;");
            migrationBuilder.CreateIndex("IX_PointsTransactions_OrderId", "PointsTransactions", "OrderId");
            migrationBuilder.CreateIndex("IX_PointsTransactions_UserId_CreatedAtUtc", "PointsTransactions", new[] { "UserId", "CreatedAtUtc" });
            migrationBuilder.CreateIndex("IX_PointsTransactions_WalletId", "PointsTransactions", "WalletId");
            migrationBuilder.CreateIndex("IX_Referrals_ReferredUserId", "Referrals", "ReferredUserId", unique: true);
            migrationBuilder.CreateIndex("IX_Referrals_ReferrerUserId_CreatedAtUtc", "Referrals", new[] { "ReferrerUserId", "CreatedAtUtc" });
            migrationBuilder.CreateIndex("IX_UserGifts_ReferralId", "UserGifts", "ReferralId");
            migrationBuilder.CreateIndex("IX_UserGifts_UserId_CreatedAtUtc", "UserGifts", new[] { "UserId", "CreatedAtUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("UserGifts");
            migrationBuilder.DropTable("PointsTransactions");
            migrationBuilder.DropTable("Referrals");
            migrationBuilder.DropTable("PointsWallets");
            migrationBuilder.DropIndex("IX_Users_Phone", "Users");
            migrationBuilder.DropIndex("IX_Users_ReferralCode", "Users");
            migrationBuilder.DropColumn("ReferralCode", "Users");
            migrationBuilder.DropColumn("ReferredByUserId", "Users");
            migrationBuilder.DropColumn("DeliveryFeeIqd", "Orders");
            migrationBuilder.DropColumn("DeliveryFeeUsd", "Orders");
            migrationBuilder.DropColumn("CustomerNote", "Orders");
            migrationBuilder.DropColumn("AdminNote", "Orders");
            migrationBuilder.DropColumn("PointsEarned", "Orders");
            migrationBuilder.DropColumn("PointsAwarded", "Orders");
            migrationBuilder.DropColumn("PointsAwardedAtUtc", "Orders");
        }
    }
}
