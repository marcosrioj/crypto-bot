using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class UpdateTradingEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "OrderHistories");

            migrationBuilder.DropColumn(
                name: "Average",
                table: "OrderHistories");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "OrderHistories",
                newName: "UsdtPriceTo");

            migrationBuilder.RenameColumn(
                name: "Executed",
                table: "OrderHistories",
                newName: "UsdtPriceFrom");

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    WhatToDo = table.Column<int>(type: "int", nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: true),
                    Strategies = table.Column<string>(type: "varchar(300)", nullable: true),
                    InvestorProfile = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    DataLearned = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.RenameColumn(
                name: "UsdtPriceTo",
                table: "OrderHistories",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "UsdtPriceFrom",
                table: "OrderHistories",
                newName: "Executed");

            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "OrderHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Average",
                table: "OrderHistories",
                type: "decimal(18,8)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
