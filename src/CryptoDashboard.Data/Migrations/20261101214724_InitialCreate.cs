using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_coin",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    symbol = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_coin", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_news",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    text = table.Column<string>(type: "TEXT", nullable: false),
                    url = table.Column<string>(type: "TEXT", nullable: false),
                    source = table.Column<string>(type: "TEXT", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    sentiment = table.Column<string>(type: "TEXT", nullable: true),
                    score = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_news", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_price_point",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    coin_id = table.Column<string>(type: "TEXT", nullable: false),
                    price = table.Column<decimal>(type: "TEXT", nullable: false),
                    market_cap = table.Column<decimal>(type: "TEXT", nullable: true),
                    variation_24h = table.Column<decimal>(type: "TEXT", nullable: true),
                    collected_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_price_point", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_price_point_tb_coin_coin_id",
                        column: x => x.coin_id,
                        principalTable: "tb_coin",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_news_coin",
                columns: table => new
                {
                    news_item_id = table.Column<long>(type: "INTEGER", nullable: false),
                    coin_id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_news_coin", x => new { x.news_item_id, x.coin_id });
                    table.ForeignKey(
                        name: "FK_tb_news_coin_tb_coin_coin_id",
                        column: x => x.coin_id,
                        principalTable: "tb_coin",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_news_coin_tb_news_news_item_id",
                        column: x => x.news_item_id,
                        principalTable: "tb_news",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_news_url",
                table: "tb_news",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_news_coin_coin_id",
                table: "tb_news_coin",
                column: "coin_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_price_point_coin_id_collected_at",
                table: "tb_price_point",
                columns: new[] { "coin_id", "collected_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_news_coin");

            migrationBuilder.DropTable(
                name: "tb_price_point");

            migrationBuilder.DropTable(
                name: "tb_news");

            migrationBuilder.DropTable(
                name: "tb_coin");
        }
    }
}
