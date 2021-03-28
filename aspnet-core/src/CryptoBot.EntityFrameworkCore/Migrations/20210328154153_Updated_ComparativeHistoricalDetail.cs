using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_ComparativeHistoricalDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Moment",
                table: "ComparativeHistoricalDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Moment",
                table: "ComparativeHistoricalDetails");
        }
    }
}
