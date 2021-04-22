using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_Formula : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Interval",
                table: "Predictions",
                newName: "IntervalToSell");

            migrationBuilder.RenameColumn(
                name: "Interval",
                table: "Formulas",
                newName: "IntervalToSell");

            migrationBuilder.AddColumn<int>(
                name: "IntervalToBuy",
                table: "Predictions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BookOrdersAction",
                table: "Formulas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BookOrdersFactor",
                table: "Formulas",
                type: "decimal(2,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "IntervalToBuy",
                table: "Formulas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalToBuy",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "BookOrdersAction",
                table: "Formulas");

            migrationBuilder.DropColumn(
                name: "BookOrdersFactor",
                table: "Formulas");

            migrationBuilder.DropColumn(
                name: "IntervalToBuy",
                table: "Formulas");

            migrationBuilder.RenameColumn(
                name: "IntervalToSell",
                table: "Predictions",
                newName: "Interval");

            migrationBuilder.RenameColumn(
                name: "IntervalToSell",
                table: "Formulas",
                newName: "Interval");
        }
    }
}
