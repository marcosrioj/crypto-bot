using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_Prediction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfitWay",
                table: "Predictions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TradingType",
                table: "Predictions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfitWay",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "TradingType",
                table: "Predictions");
        }
    }
}
