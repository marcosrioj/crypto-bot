using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_Robots_Formula : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderPrice",
                table: "Formulas");

            migrationBuilder.RenameColumn(
                name: "TryToSellByMinutePercentage",
                table: "Predictions",
                newName: "TryToSellByMinutePercentageOfProfit");

            migrationBuilder.RenameColumn(
                name: "TryToSellByMinutePercentage",
                table: "Formulas",
                newName: "TryToSellByMinutePercentageOfProfit");

            migrationBuilder.AddColumn<decimal>(
                name: "InitialAmount",
                table: "Robots",
                type: "decimal(18,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Predictions",
                type: "decimal(4,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "BalancePreserved",
                table: "Formulas",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxOrderPrice",
                table: "Formulas",
                type: "decimal(18,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderPricePerGroup",
                table: "Formulas",
                type: "decimal(18,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Formulas",
                type: "decimal(4,4)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialAmount",
                table: "Robots");

            migrationBuilder.DropColumn(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "MaxOrderPrice",
                table: "Formulas");

            migrationBuilder.DropColumn(
                name: "OrderPricePerGroup",
                table: "Formulas");

            migrationBuilder.DropColumn(
                name: "TryToSellByMinutePercentageOfLoss",
                table: "Formulas");

            migrationBuilder.RenameColumn(
                name: "TryToSellByMinutePercentageOfProfit",
                table: "Predictions",
                newName: "TryToSellByMinutePercentage");

            migrationBuilder.RenameColumn(
                name: "TryToSellByMinutePercentageOfProfit",
                table: "Formulas",
                newName: "TryToSellByMinutePercentage");

            migrationBuilder.AlterColumn<decimal>(
                name: "BalancePreserved",
                table: "Formulas",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");

            migrationBuilder.AddColumn<decimal>(
                name: "OrderPrice",
                table: "Formulas",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
