using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_prediction2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Score",
                table: "Predictions",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(22,12)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Score",
                table: "Predictions",
                type: "decimal(22,12)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);
        }
    }
}
