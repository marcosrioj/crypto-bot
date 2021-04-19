using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_prediction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Predictions",
                type: "decimal(22,12)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Predictions",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(22,12)");
        }
    }
}
