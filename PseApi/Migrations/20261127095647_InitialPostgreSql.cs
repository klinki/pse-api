using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PseApi.Migrations;

/// <inheritdoc />
public partial class _20261127095647_InitialPostgreSql : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False")
            .Annotation("Npgsql:CollationSchemasResolved", true);

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
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                BIC = table.Column<string>(type: "varchar(255)", nullable: true, collation: "case_insensitive"),
                Name = table.Column<string>(type: "varchar(255)", nullable: true),
                ISIN = table.Column<string>(type: "varchar(255)", nullable: true, collation: "case_insensitive")
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
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ISIN = table.Column<string>(type: "varchar(255)", nullable: true, collation: "case_insensitive"),
                Name = table.Column<string>(type: "varchar(255)", nullable: true),
                BIC = table.Column<string>(type: "varchar(255)", nullable: true, collation: "case_insensitive"),
                Date = table.Column<DateTime>(type: "date", nullable: false),
                Close = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                Change = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                Previous = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                YearMin = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                YearMax = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                Volume = table.Column<long>(type: "bigint", nullable: false),
                TradedAmount = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                LastTrade = table.Column<DateTime>(type: "date", nullable: false),
                MarketGroup = table.Column<string>(type: "varchar(255)", nullable: true),
                Mode = table.Column<string>(type: "varchar(255)", nullable: true),
                MarketCode = table.Column<string>(type: "varchar(255)", nullable: true),
                DayMin = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                DayMax = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
                Open = table.Column<decimal>(type: "numeric(16,4)", nullable: false),
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

        migrationBuilder.CreateIndex(
            name: "IX_trades_ISIN_Date",
            table: "trades",
            columns: new[] { "ISIN", "Date" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "days");

        migrationBuilder.DropTable(
            name: "Stocks");

        migrationBuilder.DropTable(
            name: "trades");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False")
            .OldAnnotation("Npgsql:CollationSchemasResolved", true);
    }
}
