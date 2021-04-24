using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_Formula_Added_TryToSellByMinute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TryToSellByMinute",
                table: "Formulas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TryToSellByMinutePercentage",
                table: "Formulas",
                type: "decimal(2,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TryToSellByMinute",
                table: "Formulas");

            migrationBuilder.DropColumn(
                name: "TryToSellByMinutePercentage",
                table: "Formulas");
        }
    }
}
