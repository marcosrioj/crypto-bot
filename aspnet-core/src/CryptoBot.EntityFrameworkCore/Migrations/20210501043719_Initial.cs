using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoBot.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Formulas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Currencies = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Strategy1 = table.Column<int>(type: "int", nullable: false),
                    InvestorProfile1 = table.Column<int>(type: "int", nullable: false),
                    Strategy2 = table.Column<int>(type: "int", nullable: true),
                    InvestorProfile2 = table.Column<int>(type: "int", nullable: true),
                    Strategy3 = table.Column<int>(type: "int", nullable: true),
                    InvestorProfile3 = table.Column<int>(type: "int", nullable: true),
                    IntervalToBuy = table.Column<int>(type: "int", nullable: false),
                    IntervalToSell = table.Column<int>(type: "int", nullable: false),
                    BookOrdersAction = table.Column<int>(type: "int", nullable: false),
                    BookOrdersFactor = table.Column<decimal>(type: "decimal(2,0)", nullable: false),
                    LimitOfBookOrders = table.Column<int>(type: "int", nullable: false),
                    LimitOfDataToLearn = table.Column<int>(type: "int", nullable: false),
                    BalancePreserved = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    OrderPriceType = table.Column<int>(type: "int", nullable: false),
                    ProfitWay = table.Column<int>(type: "int", nullable: false),
                    TradingType = table.Column<int>(type: "int", nullable: false),
                    MaxOrderPrice = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    OrderPricePerGroup = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    StopLimit = table.Column<int>(type: "int", nullable: false),
                    StopLimitPercentageOfProfit = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    StopLimitPercentageOfLoss = table.Column<decimal>(type: "decimal(4,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formulas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<int>(type: "int", nullable: false),
                    To = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UsdtPriceFrom = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    UsdtPriceTo = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    OriginOrderId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Orders_OriginOrderId",
                        column: x => x.OriginOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    WhatToDo = table.Column<int>(type: "int", nullable: false),
                    Strategy1 = table.Column<int>(type: "int", nullable: false),
                    InvestorProfile1 = table.Column<int>(type: "int", nullable: false),
                    Strategy2 = table.Column<int>(type: "int", nullable: true),
                    InvestorProfile2 = table.Column<int>(type: "int", nullable: true),
                    Strategy3 = table.Column<int>(type: "int", nullable: true),
                    InvestorProfile3 = table.Column<int>(type: "int", nullable: true),
                    IntervalToBuy = table.Column<int>(type: "int", nullable: false),
                    IntervalToSell = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<string>(type: "varchar(100)", nullable: true),
                    DataLearned = table.Column<int>(type: "int", nullable: false),
                    StopLimit = table.Column<int>(type: "int", nullable: false),
                    StopLimitPercentageOfProfit = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    StopLimitPercentageOfLoss = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Robots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    InitialAmount = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    FormulaId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Robots_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Robots_Formulas_FormulaId",
                        column: x => x.FormulaId,
                        principalTable: "Formulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PredictionOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    PredictionId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictionOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredictionOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PredictionOrders_Predictions_PredictionId",
                        column: x => x.PredictionId,
                        principalTable: "Predictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OriginOrderId",
                table: "Orders",
                column: "OriginOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionOrders_OrderId",
                table: "PredictionOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionOrders_PredictionId",
                table: "PredictionOrders",
                column: "PredictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Robots_FormulaId",
                table: "Robots",
                column: "FormulaId");

            migrationBuilder.CreateIndex(
                name: "IX_Robots_UserId",
                table: "Robots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PredictionOrders");

            migrationBuilder.DropTable(
                name: "Robots");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.DropTable(
                name: "Formulas");
        }
    }
}
