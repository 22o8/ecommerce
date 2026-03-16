using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations;

public partial class AddBrands : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Brands",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                LogoUrl = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Brands", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Brands_Slug",
            table: "Brands",
            column: "Slug",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Brands");
    }
}
