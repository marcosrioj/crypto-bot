using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Updated_Structure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PredictionOrders_AbpUsers_UserId",
                table: "PredictionOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PredictionOrders_Predictions_PredictionId",
                table: "PredictionOrders");

            migrationBuilder.DropIndex(
                name: "IX_PredictionOrders_UserId",
                table: "PredictionOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PredictionOrders");

            migrationBuilder.DropColumn(
                name: "MyProperty",
                table: "Orders");

            migrationBuilder.AlterColumn<long>(
                name: "PredictionId",
                table: "PredictionOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PredictionOrders_Predictions_PredictionId",
                table: "PredictionOrders",
                column: "PredictionId",
                principalTable: "Predictions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PredictionOrders_Predictions_PredictionId",
                table: "PredictionOrders");

            migrationBuilder.AlterColumn<long>(
                name: "PredictionId",
                table: "PredictionOrders",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "PredictionOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "MyProperty",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PredictionOrders_UserId",
                table: "PredictionOrders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PredictionOrders_AbpUsers_UserId",
                table: "PredictionOrders",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PredictionOrders_Predictions_PredictionId",
                table: "PredictionOrders",
                column: "PredictionId",
                principalTable: "Predictions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
