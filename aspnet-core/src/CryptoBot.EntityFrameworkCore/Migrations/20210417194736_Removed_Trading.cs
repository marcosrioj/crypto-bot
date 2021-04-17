using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Removed_Trading : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trading");

            migrationBuilder.AddColumn<long>(
                name: "OriginOrderId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OriginOrderId",
                table: "Orders",
                column: "OriginOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Orders_OriginOrderId",
                table: "Orders",
                column: "OriginOrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Orders_OriginOrderId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OriginOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OriginOrderId",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "Trading",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    EndBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    StartBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WalletId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trading_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trading_WalletId",
                table: "Trading",
                column: "WalletId");
        }
    }
}
