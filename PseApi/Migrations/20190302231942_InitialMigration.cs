using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PseApi.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "days",
                columns: table => new
                {
                    Day = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_days", x => x.Day);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BIC = table.Column<string>(type: "varchar(255)", nullable: true),
                    Name = table.Column<string>(type: "varchar(255)", nullable: true),
                    ISIN = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ISIN = table.Column<string>(type: "varchar(255)", nullable: true),
                    Name = table.Column<string>(type: "varchar(255)", nullable: true),
                    BIC = table.Column<string>(type: "varchar(255)", nullable: true),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Close = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    Change = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    Previous = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    YearMin = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    YearMax = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    TradedAmount = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    LastTrade = table.Column<DateTime>(type: "date", nullable: false),
                    MarketGroup = table.Column<string>(type: "varchar(255)", nullable: true),
                    Mode = table.Column<string>(type: "varchar(255)", nullable: true),
                    MarketCode = table.Column<string>(type: "varchar(255)", nullable: true),
                    DayMin = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    DayMax = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    Open = table.Column<decimal>(type: "decimal(16,4)", nullable: false),
                    LotSize = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_BIC",
                table: "Stocks",
                column: "BIC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ISIN",
                table: "Stocks",
                column: "ISIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trades_BIC",
                table: "trades",
                column: "BIC");

            migrationBuilder.CreateIndex(
                name: "IX_trades_Date",
                table: "trades",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_trades_ISIN",
                table: "trades",
                column: "ISIN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "days");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "trades");
        }
    }
}
