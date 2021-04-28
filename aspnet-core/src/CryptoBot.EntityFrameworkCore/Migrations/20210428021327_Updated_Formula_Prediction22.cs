using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_Formula_Prediction22 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfProfit",
                table: "Predictions",
                type: "decimal(2,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Predictions",
                type: "decimal(2,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfProfit",
                table: "Formulas",
                type: "decimal(2,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Formulas",
                type: "decimal(2,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,4)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfProfit",
                table: "Predictions",
                type: "decimal(4,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(2,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Predictions",
                type: "decimal(4,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(2,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfProfit",
                table: "Formulas",
                type: "decimal(4,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(2,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Formulas",
                type: "decimal(4,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(2,2)");
        }
    }
}
