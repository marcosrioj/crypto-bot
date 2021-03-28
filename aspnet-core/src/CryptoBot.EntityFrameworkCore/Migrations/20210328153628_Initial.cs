using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComparativeHistoricals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparativeHistoricals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupComparativeHistoricals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApproachTrading = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    LimitOfDetails = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupComparativeHistoricals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<int>(type: "int", nullable: false),
                    To = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Average = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Executed = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuotationHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    MomentReference = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparativeHistoricalDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PercentageGainFromPreviousDetail = table.Column<decimal>(type: "decimal(8,8)", nullable: false),
                    PercentageGainFromFirstDetail = table.Column<decimal>(type: "decimal(8,8)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    ComparativeHistoricalId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparativeHistoricalDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparativeHistoricalDetails_ComparativeHistoricals_ComparativeHistoricalId",
                        column: x => x.ComparativeHistoricalId,
                        principalTable: "ComparativeHistoricals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparativeHistoricalDetails_ComparativeHistoricalId",
                table: "ComparativeHistoricalDetails",
                column: "ComparativeHistoricalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparativeHistoricalDetails");

            migrationBuilder.DropTable(
                name: "GroupComparativeHistoricals");

            migrationBuilder.DropTable(
                name: "OrderHistories");

            migrationBuilder.DropTable(
                name: "QuotationHistories");

            migrationBuilder.DropTable(
                name: "ComparativeHistoricals");
        }
    }
}
