using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Added_ComparativeHistorical : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComparativeHistoricals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApproachTrading = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    LimitOfDetails = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparativeHistoricals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparativeHistoricalDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PercentageGainFromPreviousDetail = table.Column<decimal>(type: "decimal(3,3)", nullable: false),
                    PercentageGainFromFirstDetail = table.Column<decimal>(type: "decimal(3,3)", nullable: false),
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
                name: "ComparativeHistoricals");
        }
    }
}
