using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_Prediction_Added_TryToSellByMinute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TryToSellByMinute",
                table: "Predictions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TryToSellByMinutePercentage",
                table: "Predictions",
                type: "decimal(2,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TryToSellByMinute",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "TryToSellByMinutePercentage",
                table: "Predictions");
        }
    }
}
